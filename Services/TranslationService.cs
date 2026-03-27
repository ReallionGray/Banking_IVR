using System.Text.RegularExpressions;

namespace Banking_IVR.Services;

public partial class TranslationService : ITranslationService
{
    private static readonly Dictionary<string, string> PidginTranslations = new(StringComparer.Ordinal)
    {
        ["Press 1 for balance. Press 2 for transfer."] = "Press 1 to know how much remain for your account. Press 2 if you wan transfer money.",
        ["Enter the 10 digit recipient account number."] = "Enter the 10 digit account number wey you wan send money to.",
        ["The account number entered is invalid. Please enter a valid 10 digit account number."] = "The account number no correct. Abeg enter correct 10 digit account number.",
        ["Enter transfer amount in naira, then press the hash key."] = "Enter the amount for naira, then press the hash key.",
        ["Enter your 4 digit transfer PIN."] = "Enter your 4 digit transfer PIN.",
        ["Transfer cancelled. Press 1 for menu or 2 to end."] = "Transfer don cancel. Press 1 for menu or 2 to end call.",
        ["Invalid selection. Press 1 to continue or 2 to cancel."] = "That choice no correct. Press 1 to continue or 2 to cancel.",
        ["Invalid PIN. Press 1 for menu or 2 to end."] = "PIN no correct. Press 1 for menu or 2 to end call.",
        ["Transfer failed due to insufficient balance. Press 1 for menu or 2 to end."] = "Transfer fail because balance no enough. Press 1 for menu or 2 to end call.",
        ["Thank you for using the banking service."] = "Thank you for using our banking service.",
        ["Unable to process this call because the phone number is missing."] = "We no fit process this call because phone number no dey."
    };

    private static readonly Dictionary<string, string> YorubaTranslations = new(StringComparer.Ordinal)
    {
        ["Press 1 for balance. Press 2 for transfer."] = "Tẹ 1 fun iye to ku. Tẹ 2 fun gbigbe owo.",
        ["Enter the 10 digit recipient account number."] = "Tẹ nọmba akanti olugba oni nọmba mẹwa.",
        ["The account number entered is invalid. Please enter a valid 10 digit account number."] = "Nọmba akanti ti o tẹ ko pe. Jọwọ tẹ nọmba akanti oni nọmba mẹwa to pe.",
        ["Enter transfer amount in naira, then press the hash key."] = "Tẹ iye owo ni naira, lẹhinna tẹ bọtini hash.",
        ["Enter your 4 digit transfer PIN."] = "Tẹ PIN gbigbe owo oni nọmba mẹrin rẹ.",
        ["Transfer cancelled. Press 1 for menu or 2 to end."] = "A ti fagile gbigbe owo. Tẹ 1 fun akojọ tabi 2 lati pari.",
        ["Invalid selection. Press 1 to continue or 2 to cancel."] = "Aṣayan ti o yan ko pe. Tẹ 1 lati tẹsiwaju tabi 2 lati fagile.",
        ["Invalid PIN. Press 1 for menu or 2 to end."] = "PIN ko pe. Tẹ 1 fun akojọ tabi 2 lati pari.",
        ["Transfer failed due to insufficient balance. Press 1 for menu or 2 to end."] = "Gbigbe owo kuna nitori iye to ku ko to. Tẹ 1 fun akojọ tabi 2 lati pari.",
        ["Thank you for using the banking service."] = "O ṣeun fun lilo iṣẹ ile-ifowopamọ wa.",
        ["Unable to process this call because the phone number is missing."] = "A ko le ṣiṣẹ ipe yii nitori nọmba foonu ko si."
    };

    private static readonly Dictionary<string, string> IgboTranslations = new(StringComparer.Ordinal)
    {
        ["Press 1 for balance. Press 2 for transfer."] = "Pịa 1 maka ego fọdụrụ. Pịa 2 maka izipu ego.",
        ["Enter the 10 digit recipient account number."] = "Tinye nọmba akaụntụ onye nnata nke nwere ọnụọgụ iri.",
        ["The account number entered is invalid. Please enter a valid 10 digit account number."] = "Nọmba akaụntụ i tinyere ezighi ezi. Biko tinye nọmba akaụntụ ziri ezi nke nwere ọnụọgụ iri.",
        ["Enter transfer amount in naira, then press the hash key."] = "Tinye ego ịchọrọ izipu na naira, mechaa pịa bọtịnụ hash.",
        ["Enter your 4 digit transfer PIN."] = "Tinye PIN izipu ego gị nke nwere ọnụọgụ anọ.",
        ["Transfer cancelled. Press 1 for menu or 2 to end."] = "A kagburu izipu ego. Pịa 1 maka menu ma ọ bụ 2 iji kwụsị.",
        ["Invalid selection. Press 1 to continue or 2 to cancel."] = "Nhọrọ ahụ ezighi ezi. Pịa 1 iji gaa n'ihu ma ọ bụ 2 iji kagbuo.",
        ["Invalid PIN. Press 1 for menu or 2 to end."] = "PIN ezighi ezi. Pịa 1 maka menu ma ọ bụ 2 iji kwụsị.",
        ["Transfer failed due to insufficient balance. Press 1 for menu or 2 to end."] = "Izipu ego dara n'ihi na ego fọdụrụ ezughị. Pịa 1 maka menu ma ọ bụ 2 iji kwụsị.",
        ["Thank you for using the banking service."] = "Daalụ maka iji ọrụ ụlọ akụ anyị.",
        ["Unable to process this call because the phone number is missing."] = "Anyi enweghị ike ịrụ ọrụ oku a n'ihi na nọmba ekwentị adịghị."
    };

    private static readonly Dictionary<string, string> HausaTranslations = new(StringComparer.Ordinal)
    {
        ["Press 1 for balance. Press 2 for transfer."] = "Danna 1 don jin adadin kuɗin da ya rage. Danna 2 don tura kuɗi.",
        ["Enter the 10 digit recipient account number."] = "Shigar da lambar asusun mai karɓa mai lambobi goma.",
        ["The account number entered is invalid. Please enter a valid 10 digit account number."] = "Lambar asusun da ka shigar ba daidai ba ce. Da fatan a shigar da ingantacciyar lambar asusu mai lambobi goma.",
        ["Enter transfer amount in naira, then press the hash key."] = "Shigar da adadin kuɗin a naira, sannan danna maɓallin hash.",
        ["Enter your 4 digit transfer PIN."] = "Shigar da PIN ɗin tura kuɗinka mai lambobi huɗu.",
        ["Transfer cancelled. Press 1 for menu or 2 to end."] = "An soke tura kuɗi. Danna 1 don menu ko 2 don ƙarewa.",
        ["Invalid selection. Press 1 to continue or 2 to cancel."] = "Zaɓin da aka yi ba daidai ba ne. Danna 1 don ci gaba ko 2 don sokewa.",
        ["Invalid PIN. Press 1 for menu or 2 to end."] = "PIN ba daidai ba ne. Danna 1 don menu ko 2 don ƙarewa.",
        ["Transfer failed due to insufficient balance. Press 1 for menu or 2 to end."] = "Tura kuɗi ya gaza saboda kuɗin da ya rage bai isa ba. Danna 1 don menu ko 2 don ƙarewa.",
        ["Thank you for using the banking service."] = "Mun gode da amfani da sabis na bankinmu.",
        ["Unable to process this call because the phone number is missing."] = "Ba za mu iya sarrafa wannan kiran ba saboda babu lambar waya."
    };

    public string Translate(string text, string language)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return language switch
        {
            "pidgin" => TranslatePidgin(text),
            "yo" => TranslateWithTemplates(text, YorubaTranslations, "yo"),
            "ig" => TranslateWithTemplates(text, IgboTranslations, "ig"),
            "ha" => TranslateWithTemplates(text, HausaTranslations, "ha"),
            _ => text
        };
    }

    private static string TranslatePidgin(string text)
    {
        if (PidginTranslations.TryGetValue(text, out var translated))
        {
            return translated;
        }

        return TranslateWithTemplates(text, PidginTranslations, "pidgin");
    }

    private static string TranslateWithTemplates(string text, IReadOnlyDictionary<string, string> staticTranslations, string language)
    {
        if (staticTranslations.TryGetValue(text, out var translated))
        {
            return translated;
        }

        var balanceMatch = BalanceRegex().Match(text);
        if (balanceMatch.Success)
        {
            return language switch
            {
                "pidgin" => $"Your balance na {balanceMatch.Groups["amount"].Value} naira. Press 1 for menu or 2 to end call.",
                "yo" => $"Iye owo to ku ni {balanceMatch.Groups["amount"].Value} naira. Tẹ 1 fun akojọ tabi 2 lati pari.",
                "ig" => $"Ego fọdụrụ gị bụ {balanceMatch.Groups["amount"].Value} naira. Pịa 1 maka menu ma ọ bụ 2 iji kwụsị.",
                "ha" => $"Adadin kuɗinka da ya rage shi ne naira {balanceMatch.Groups["amount"].Value}. Danna 1 don menu ko 2 don ƙarewa.",
                _ => text
            };
        }

        var recipientMatch = RecipientRegex().Match(text);
        if (recipientMatch.Success)
        {
            var name = recipientMatch.Groups["name"].Value;
            return language switch
            {
                "pidgin" => $"Recipient na {name}. Press 1 to continue or 2 to enter account number again.",
                "yo" => $"Olugba ni {name}. Tẹ 1 lati tẹsiwaju tabi 2 lati tun tẹ nọmba akanti.",
                "ig" => $"Onye nnata bụ {name}. Pịa 1 iji gaa n'ihu ma ọ bụ 2 iji tinye nọmba akaụntụ ọzọ.",
                "ha" => $"Mai karɓa shi ne {name}. Danna 1 don ci gaba ko 2 don sake shigar da lambar asusu.",
                _ => text
            };
        }

        var transferConfirmMatch = TransferConfirmRegex().Match(text);
        if (transferConfirmMatch.Success)
        {
            var amount = transferConfirmMatch.Groups["amount"].Value;
            var name = transferConfirmMatch.Groups["name"].Value;
            var account = transferConfirmMatch.Groups["account"].Value;
            return language switch
            {
                "pidgin" => $"You wan send {amount} naira go {name}, account number {account}. Press 1 to continue or 2 to cancel.",
                "yo" => $"O fẹ fi {amount} naira ranṣẹ si {name}, nọmba akanti {account}. Tẹ 1 lati tẹsiwaju tabi 2 lati fagile.",
                "ig" => $"Ị na-achọ izipu {amount} naira nye {name}, nọmba akaụntụ {account}. Pịa 1 iji gaa n'ihu ma ọ bụ 2 iji kagbuo.",
                "ha" => $"Kana shirin tura naira {amount} zuwa ga {name}, lambar asusu {account}. Danna 1 don ci gaba ko 2 don sokewa.",
                _ => text
            };
        }

        var invalidAmountMatch = InvalidAmountRegex().Match(text);
        if (invalidAmountMatch.Success)
        {
            var max = invalidAmountMatch.Groups["max"].Value;
            return language switch
            {
                "pidgin" => $"Amount no correct. Enter amount between 1 and {max} naira, then press hash key.",
                "yo" => $"Iye owo ko pe. Tẹ iye laarin 1 ati {max} naira, lẹhinna tẹ bọtini hash.",
                "ig" => $"Ego ahụ ezighi ezi. Tinye ego dị n'etiti 1 na {max} naira, mechaa pịa bọtịnụ hash.",
                "ha" => $"Adadin bai daidai ba. Shigar da adadi tsakanin 1 da {max} naira, sannan danna hash.",
                _ => text
            };
        }

        var transferSuccessMatch = TransferSuccessRegex().Match(text);
        if (transferSuccessMatch.Success)
        {
            var amount = transferSuccessMatch.Groups["amount"].Value;
            var name = transferSuccessMatch.Groups["name"].Value;
            return language switch
            {
                "pidgin" => $"Transfer of {amount} naira to {name} successful. Press 1 for menu or 2 to end call.",
                "yo" => $"Gbigbe owo {amount} naira si {name} ṣaṣeyọri. Tẹ 1 fun akojọ tabi 2 lati pari.",
                "ig" => $"Izipu ego {amount} naira nye {name} gara nke ọma. Pịa 1 maka menu ma ọ bụ 2 iji kwụsị.",
                "ha" => $"An yi nasarar tura naira {amount} zuwa ga {name}. Danna 1 don menu ko 2 don ƙarewa.",
                _ => text
            };
        }

        var welcomeMatch = WelcomeWithPhoneRegex().Match(text);
        if (welcomeMatch.Success)
        {
            var phone = welcomeMatch.Groups["phone"].Value;
            return language switch
            {
                "pidgin" => $"Welcome to the banking service. We dey talk with phone number {phone}. Press 1 for English. Press 2 for Pidgin. Press 3 for Yoruba. Press 4 for Igbo. Press 5 for Hausa.",
                "yo" => $"E kaabo si ise ifowopamo wa. A n ba nọmba foonu {phone} sọrọ. Tẹ 1 fun Gẹẹsi. Tẹ 2 fun Pidgin. Tẹ 3 fun Yoruba. Tẹ 4 fun Igbo. Tẹ 5 fun Hausa.",
                "ig" => $"Nnoo na ọrụ banki anyi. Anyị na-ekwu na nọmba ekwentị {phone}. Pịa 1 maka Bekee. Pịa 2 maka Pidgin. Pịa 3 maka Yoruba. Pịa 4 maka Igbo. Pịa 5 maka Hausa.",
                "ha" => $"Barka da zuwa sabis din bankinmu. Muna magana da lambar waya {phone}. Danna 1 don Turanci. Danna 2 don Pidgin. Danna 3 don Yoruba. Danna 4 don Igbo. Danna 5 don Hausa.",
                _ => text
            };
        }

        return text;
    }

    [GeneratedRegex(@"^Your balance is (?<amount>[\d,]+) naira\. Press 1 for menu or 2 to end\.$")]
    private static partial Regex BalanceRegex();

    [GeneratedRegex(@"^Recipient is (?<name>.+)\. Press 1 to continue or 2 to re-enter account number\.$")]
    private static partial Regex RecipientRegex();

    [GeneratedRegex(@"^You are about to send (?<amount>[\d,]+) naira to (?<name>.+), account number (?<account>\d+)\. Press 1 to continue or 2 to cancel\.$")]
    private static partial Regex TransferConfirmRegex();

    [GeneratedRegex(@"^Invalid amount\. Enter an amount between 1 and (?<max>[\d,]+) naira, then press the hash key\.$")]
    private static partial Regex InvalidAmountRegex();

    [GeneratedRegex(@"^Transfer of (?<amount>[\d,]+) naira to (?<name>.+) successful\. Press 1 for menu or 2 to end\.$")]
    private static partial Regex TransferSuccessRegex();

    [GeneratedRegex(@"^Welcome to the banking service\. We are speaking with phone number (?<phone>.+)\. Press 1 for English\. Press 2 for Pidgin\. Press 3 for Yoruba\. Press 4 for Igbo\. Press 5 for Hausa\.$")]
    private static partial Regex WelcomeWithPhoneRegex();
}
