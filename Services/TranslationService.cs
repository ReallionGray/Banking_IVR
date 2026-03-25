namespace Banking_IVR.Services;

public class TranslationService : ITranslationService
{
    public string Translate(string text, string language)
        => string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : language switch
        {
            "pidgin" => $"Pidgin: {text}",
            "yo" => $"Yoruba: {text}",
            "ig" => $"Igbo: {text}",
            "ha" => $"Hausa: {text}",
            _ => text
        };
}
