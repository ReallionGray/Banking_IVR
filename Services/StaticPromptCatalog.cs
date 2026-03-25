namespace Banking_IVR.Services;

public static class StaticPromptCatalog
{
    public static readonly IReadOnlyDictionary<string, string> EnglishPrompts = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["welcome"] = "Welcome to the banking service. Press 1 for English. Press 2 for Pidgin. Press 3 for Yoruba. Press 4 for Igbo. Press 5 for Hausa.",
        ["menu"] = "Press 1 for balance. Press 2 for transfer.",
        ["enter-recipient"] = "Enter the 10 digit recipient account number.",
        ["invalid-account"] = "The account number entered is invalid. Please enter a valid 10 digit account number.",
        ["enter-amount"] = "Enter transfer amount in naira, then press the hash key.",
        ["enter-pin"] = "Enter your 4 digit transfer PIN.",
        ["transfer-cancelled"] = "Transfer cancelled. Press 1 for menu or 2 to end.",
        ["invalid-selection"] = "Invalid selection. Press 1 to continue or 2 to cancel.",
        ["invalid-pin"] = "Invalid PIN. Press 1 for menu or 2 to end.",
        ["thank-you"] = "Thank you for using the banking service.",
        ["missing-phone"] = "Unable to process this call because the phone number is missing."
    };

    public static readonly string[] AudioLanguages = ["pidgin", "yo", "ig", "ha"];
}
