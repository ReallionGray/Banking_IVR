using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

namespace Banking_IVR.Services;

public class GoogleTtsAudioPromptProvider : IAudioPromptProvider
{
    private readonly HttpClient _httpClient;
    private readonly IvrOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GoogleTtsAudioPromptProvider> _logger;
    private const string GoogleScope = "https://www.googleapis.com/auth/cloud-platform";
    private const string TokenCacheKey = "google_tts_access_token";

    public GoogleTtsAudioPromptProvider(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<IvrOptions> options,
        ILogger<GoogleTtsAudioPromptProvider> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.GoogleServiceAccountJson);

    public async Task<bool> TryGenerateAsync(string language, string text, string outputPath, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Google TTS provider selected but Google service account JSON is not configured.");
            return false;
        }

        var accessToken = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        var requestBody = new GoogleTtsRequest(
            new SynthesisInput(text),
            new VoiceSelectionParams(
                MapGoogleLanguageCode(language),
                string.IsNullOrWhiteSpace(_options.GoogleTtsVoiceName) ? null : _options.GoogleTtsVoiceName),
            new AudioConfig("MP3"));

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/text:synthesize");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(requestBody);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to generate Google TTS audio for language {Language}. Status {StatusCode}. Response: {Response}", language, response.StatusCode, error);
            return false;
        }

        var payload = await response.Content.ReadFromJsonAsync<GoogleTtsResponse>(cancellationToken: cancellationToken);
        if (payload is null || string.IsNullOrWhiteSpace(payload.AudioContent))
        {
            _logger.LogWarning("Google TTS returned no audio content for language {Language}.", language);
            return false;
        }

        var audioBytes = Convert.FromBase64String(payload.AudioContent);
        await File.WriteAllBytesAsync(outputPath, audioBytes, cancellationToken);
        return true;
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cachedToken) && !string.IsNullOrWhiteSpace(cachedToken))
        {
            return cachedToken;
        }

        if (string.IsNullOrWhiteSpace(_options.GoogleServiceAccountJson))
        {
            _logger.LogWarning("Google TTS provider selected but GoogleServiceAccountJson is missing.");
            return null;
        }

        GoogleServiceAccount? serviceAccount;
        try
        {
            serviceAccount = JsonSerializer.Deserialize<GoogleServiceAccount>(_options.GoogleServiceAccountJson);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse Google service account JSON.");
            return null;
        }

        if (serviceAccount is null ||
            string.IsNullOrWhiteSpace(serviceAccount.ClientEmail) ||
            string.IsNullOrWhiteSpace(serviceAccount.PrivateKey) ||
            string.IsNullOrWhiteSpace(serviceAccount.TokenUri))
        {
            _logger.LogWarning("Google service account JSON is missing required fields.");
            return null;
        }

        var jwt = CreateSignedJwt(serviceAccount);
        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, serviceAccount.TokenUri)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"] = jwt
            })
        };

        using var tokenResponse = await _httpClient.SendAsync(tokenRequest, cancellationToken);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            var error = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to obtain Google access token. Status {StatusCode}. Response: {Response}", tokenResponse.StatusCode, error);
            return null;
        }

        var payload = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);
        if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
        {
            _logger.LogWarning("Google token endpoint returned no access token.");
            return null;
        }

        _cache.Set(TokenCacheKey, payload.AccessToken, TimeSpan.FromSeconds(Math.Max(60, payload.ExpiresIn - 60)));
        return payload.AccessToken;
    }

    private static string CreateSignedJwt(GoogleServiceAccount serviceAccount)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object>
        {
            ["alg"] = "RS256",
            ["typ"] = "JWT"
        }));

        var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object>
        {
            ["iss"] = serviceAccount.ClientEmail!,
            ["scope"] = GoogleScope,
            ["aud"] = serviceAccount.TokenUri!,
            ["iat"] = now,
            ["exp"] = now + 3600
        }));

        var unsignedToken = $"{header}.{payload}";

        using var rsa = RSA.Create();
        rsa.ImportFromPem(serviceAccount.PrivateKey!.AsSpan());
        var signature = rsa.SignData(
            Encoding.UTF8.GetBytes(unsignedToken),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return $"{unsignedToken}.{Base64UrlEncode(signature)}";
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string MapGoogleLanguageCode(string language) => language switch
    {
        "yo" => "yo-NG",
        "ig" => "en-NG",
        "ha" => "en-NG",
        "pidgin" => "en-NG",
        _ => "en-US"
    };

    private sealed record GoogleTtsRequest(
        [property: JsonPropertyName("input")] SynthesisInput Input,
        [property: JsonPropertyName("voice")] VoiceSelectionParams Voice,
        [property: JsonPropertyName("audioConfig")] AudioConfig AudioConfig);

    private sealed record SynthesisInput([property: JsonPropertyName("text")] string Text);

    private sealed record VoiceSelectionParams(
        [property: JsonPropertyName("languageCode")] string LanguageCode,
        [property: JsonPropertyName("name")] string? Name);

    private sealed record AudioConfig([property: JsonPropertyName("audioEncoding")] string AudioEncoding);

    private sealed record GoogleTtsResponse([property: JsonPropertyName("audioContent")] string AudioContent);

    private sealed record GoogleTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record GoogleServiceAccount(
        [property: JsonPropertyName("client_email")] string? ClientEmail,
        [property: JsonPropertyName("private_key")] string? PrivateKey,
        [property: JsonPropertyName("token_uri")] string? TokenUri);
}
