using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Banking_IVR.Services;

public class ElevenLabsAudioPromptProvider : IAudioPromptProvider
{
    private readonly HttpClient _httpClient;
    private readonly IvrOptions _options;
    private readonly ILogger<ElevenLabsAudioPromptProvider> _logger;

    public ElevenLabsAudioPromptProvider(
        HttpClient httpClient,
        IOptions<IvrOptions> options,
        ILogger<ElevenLabsAudioPromptProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ElevenLabsApiKey) &&
        !string.IsNullOrWhiteSpace(_options.ElevenLabsVoiceId);

    public async Task<bool> TryGenerateAsync(string language, string text, string outputPath, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("ElevenLabs provider selected but credentials are incomplete.");
            return false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"v1/text-to-speech/{_options.ElevenLabsVoiceId}?output_format=mp3_44100_128");
        request.Headers.Add("xi-api-key", _options.ElevenLabsApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));
        request.Content = JsonContent.Create(new ElevenLabsRequest(
            text,
            _options.ElevenLabsModelId,
            MapLanguageCode(language)));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to generate ElevenLabs audio for language {Language}. Status {StatusCode}. Response: {Response}", language, response.StatusCode, error);
            return false;
        }

        await using var output = File.Create(outputPath);
        await response.Content.CopyToAsync(output, cancellationToken);
        return true;
    }

    private static string? MapLanguageCode(string language) => language switch
    {
        "yo" => "yo",
        "ig" => "ig",
        "ha" => "ha",
        "pidgin" => null,
        _ => null
    };

    private sealed record ElevenLabsRequest(
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("model_id")] string ModelId,
        [property: JsonPropertyName("language_code")] string? LanguageCode);
}
