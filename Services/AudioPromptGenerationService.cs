using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Banking_IVR.Services;

public class AudioPromptGenerationService : IAudioPromptGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly ITranslationService _translator;
    private readonly IvrOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AudioPromptGenerationService> _logger;

    public AudioPromptGenerationService(
        HttpClient httpClient,
        ITranslationService translator,
        IOptions<IvrOptions> options,
        IWebHostEnvironment environment,
        ILogger<AudioPromptGenerationService> logger)
    {
        _httpClient = httpClient;
        _translator = translator;
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task GenerateStaticPromptsAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.EnableCloudAudioGeneration)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.ElevenLabsApiKey) ||
            string.IsNullOrWhiteSpace(_options.ElevenLabsVoiceId))
        {
            _logger.LogWarning("Cloud audio generation is enabled but ElevenLabs credentials are incomplete.");
            return;
        }

        Directory.CreateDirectory(GetAudioRoot());

        foreach (var language in StaticPromptCatalog.AudioLanguages)
        {
            var languageDirectory = Path.Combine(GetAudioRoot(), language);
            Directory.CreateDirectory(languageDirectory);

            foreach (var prompt in StaticPromptCatalog.EnglishPrompts)
            {
                var outputPath = Path.Combine(languageDirectory, $"{prompt.Key}.mp3");
                if (File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
                {
                    continue;
                }

                await EnsurePromptExistsAsync(language, prompt.Key, cancellationToken);
            }
        }
    }

    public async Task<string?> EnsurePromptExistsAsync(string language, string promptKey, CancellationToken cancellationToken = default)
    {
        if (!StaticPromptCatalog.EnglishPrompts.TryGetValue(promptKey, out var englishText))
        {
            return null;
        }

        var outputPath = GetPromptPath(language, promptKey);
        if (File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
        {
            return outputPath;
        }

        if (!_options.EnableCloudAudioGeneration ||
            string.IsNullOrWhiteSpace(_options.ElevenLabsApiKey) ||
            string.IsNullOrWhiteSpace(_options.ElevenLabsVoiceId))
        {
            return null;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var translatedText = _translator.Translate(englishText, language);
        var generated = await GeneratePromptAsync(language, translatedText, outputPath, cancellationToken);
        return generated ? outputPath : null;
    }

    private async Task<bool> GeneratePromptAsync(string language, string text, string outputPath, CancellationToken cancellationToken)
    {
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
            _logger.LogWarning("Failed to generate audio prompt for language {Language}. Status {StatusCode}. Response: {Response}", language, response.StatusCode, error);
            return false;
        }

        await using var output = File.Create(outputPath);
        await response.Content.CopyToAsync(output, cancellationToken);
        _logger.LogInformation("Generated cloud audio prompt {Path}", outputPath);
        return true;
    }

    private string GetAudioRoot()
        => Path.Combine(_environment.WebRootPath ?? "wwwroot", _options.AudioBasePath.Trim('/'));

    private string GetPromptPath(string language, string promptKey)
        => Path.Combine(GetAudioRoot(), language, $"{promptKey}.mp3");

    private static string? MapLanguageCode(string language) => language switch
    {
        "yo" => "yo",
        "ig" => "ig",
        "ha" => "ha",
        // Pidgin isn't a documented ISO-639-1 target for ElevenLabs API language_code.
        "pidgin" => null,
        _ => null
    };

    private sealed record ElevenLabsRequest(
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("model_id")] string ModelId,
        [property: JsonPropertyName("language_code")] string? LanguageCode);
}
