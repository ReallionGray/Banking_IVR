namespace Banking_IVR.Services;

public interface IAudioPromptProvider
{
    bool IsConfigured { get; }
    Task<bool> TryGenerateAsync(string language, string text, string outputPath, CancellationToken cancellationToken = default);
}
