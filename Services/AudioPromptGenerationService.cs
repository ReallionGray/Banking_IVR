using Microsoft.Extensions.Options;

namespace Banking_IVR.Services;

public class AudioPromptGenerationService : IAudioPromptGenerationService
{
    private readonly IAudioPromptProvider _provider;
    private readonly ITranslationService _translator;
    private readonly IvrOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AudioPromptGenerationService> _logger;

    public AudioPromptGenerationService(
        IAudioPromptProvider provider,
        ITranslationService translator,
        IOptions<IvrOptions> options,
        IWebHostEnvironment environment,
        ILogger<AudioPromptGenerationService> logger)
    {
        _provider = provider;
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

        Directory.CreateDirectory(GetAudioRoot());

        foreach (var language in StaticPromptCatalog.AudioLanguages)
        {
            var languageDirectory = Path.Combine(GetAudioRoot(), language);
            Directory.CreateDirectory(languageDirectory);

            foreach (var prompt in StaticPromptCatalog.EnglishPrompts)
            {
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

        if (!_options.EnableCloudAudioGeneration || !_provider.IsConfigured)
        {
            return null;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var translatedText = _translator.Translate(englishText, language);
        var generated = await _provider.TryGenerateAsync(language, translatedText, outputPath, cancellationToken);
        if (generated)
        {
            _logger.LogInformation("Generated cloud audio prompt {Path}", outputPath);
            return outputPath;
        }

        return null;
    }

    private string GetAudioRoot()
        => Path.Combine(_environment.WebRootPath ?? "wwwroot", _options.AudioBasePath.Trim('/'));

    private string GetPromptPath(string language, string promptKey)
        => Path.Combine(GetAudioRoot(), language, $"{promptKey}.mp3");
}
