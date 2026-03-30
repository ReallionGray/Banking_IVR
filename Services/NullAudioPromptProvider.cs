namespace Banking_IVR.Services;

public class NullAudioPromptProvider : IAudioPromptProvider
{
    public bool IsConfigured => false;

    public Task<bool> TryGenerateAsync(string language, string text, string outputPath, CancellationToken cancellationToken = default)
        => Task.FromResult(false);
}
