namespace Banking_IVR.Services;

public interface IAudioPromptGenerationService
{
    Task GenerateStaticPromptsAsync(CancellationToken cancellationToken = default);
    Task<string?> EnsurePromptExistsAsync(string language, string promptKey, CancellationToken cancellationToken = default);
}
