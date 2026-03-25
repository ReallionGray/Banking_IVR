using Banking_IVR.Services;
using Banking_IVR.Twiml;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Banking_IVR.Controllers;

[ApiController]
[Route("api/ivr")]
public class IvrController : ControllerBase
{
    private readonly ITranslationService _translator;
    private readonly IBankingService _banking;
    private readonly ISessionService _session;
    private readonly ILogger<IvrController> _logger;
    private readonly IvrOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private static readonly HashSet<string> AudioFirstLanguages = ["pidgin", "yo", "ig", "ha"];
    private static readonly HashSet<string> AudioPromptKeys =
    [
        "welcome",
        "menu",
        "enter-recipient",
        "invalid-account",
        "enter-amount",
        "enter-pin",
        "transfer-cancelled",
        "invalid-selection",
        "invalid-pin",
        "thank-you",
        "missing-phone"
    ];

    public IvrController(
        ITranslationService translator,
        IBankingService banking,
        ISessionService session,
        IOptions<IvrOptions> options,
        ILogger<IvrController> logger,
        IHostEnvironment environment,
        IWebHostEnvironment webHostEnvironment)
    {
        _translator = translator;
        _banking = banking;
        _session = session;
        _logger = logger;
        _options = options.Value;
        _environment = environment;
        _webHostEnvironment = webHostEnvironment;
    }

    private string T(string text, string lang)
        => lang == _options.DefaultLanguage ? text : _translator.Translate(text, lang);

    private static (string Voice, string Language) GetSpeechProfile(string lang) => lang switch
    {
        "en" => ("woman", "en-US"),
        "pidgin" => ("woman", "en-GB"),
        "yo" => ("woman", "en-US"),
        "ig" => ("woman", "en-US"),
        "ha" => ("woman", "en-US"),
        _ => ("woman", "en-US")
    };

    private void Say(Gather gather, string text, string lang, string? promptKey = null)
    {
        if (TryAppendAudio(gather, lang, promptKey))
        {
            return;
        }

        var profile = GetSpeechProfile(lang);
        gather.Say(text, profile.Voice, profile.Language);
    }

    private void Say(VoiceResponse response, string text, string lang, string? promptKey = null)
    {
        if (TryAppendAudio(response, lang, promptKey))
        {
            return;
        }

        var profile = GetSpeechProfile(lang);
        response.Say(text, profile.Voice, profile.Language);
    }

    private bool TryAppendAudio(Gather gather, string lang, string? promptKey)
    {
        var audioUrl = BuildAudioUrl(lang, promptKey);
        if (audioUrl is null)
        {
            return false;
        }

        gather.Play(audioUrl);
        return true;
    }

    private bool TryAppendAudio(VoiceResponse response, string lang, string? promptKey)
    {
        var audioUrl = BuildAudioUrl(lang, promptKey);
        if (audioUrl is null)
        {
            return false;
        }

        response.Play(audioUrl);
        return true;
    }

    private string? BuildAudioUrl(string lang, string? promptKey)
    {
        if (string.IsNullOrWhiteSpace(promptKey) ||
            !AudioFirstLanguages.Contains(lang) ||
            !AudioPromptKeys.Contains(promptKey))
        {
            return null;
        }

        var relativePath = $"{_options.AudioBasePath.TrimEnd('/')}/{lang}/{promptKey}.aiff";
        var physicalPath = Path.Combine(
            _webHostEnvironment.WebRootPath ?? "wwwroot",
            _options.AudioBasePath.Trim('/'),
            lang,
            $"{promptKey}.aiff");

        if (!System.IO.File.Exists(physicalPath))
        {
            _logger.LogWarning("Audio prompt file not found for language {Language} and prompt {PromptKey}: {PhysicalPath}", lang, promptKey, physicalPath);
            return null;
        }

        var baseUrl = _options.PublicBaseUrl;
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            return $"{baseUrl.TrimEnd('/')}{relativePath}";
        }

        return $"{Request.Scheme}://{Request.Host}{relativePath}";
    }

    [HttpPost("start")]
    public IActionResult Start()
    {
        if (!TryGetMsisdn(out var msisdn, out var failure))
        {
            return failure!;
        }

        _session.Initialize(msisdn);

        var res = new VoiceResponse();
        var gather = new Gather(1, "/api/ivr/set-language", "POST");
        Say(
            gather,
            "Welcome to the banking service. Press 1 for English. Press 2 for Pidgin. Press 3 for Yoruba. Press 4 for Igbo. Press 5 for Hausa.",
            _options.DefaultLanguage,
            "welcome");
        res.Append(gather);
        res.Redirect(new Uri("/api/ivr/start", UriKind.Relative));

        return Twiml(res);
    }

    [HttpPost("set-language")]
    public IActionResult SetLanguage()
    {
        if (!TryGetMsisdn(out var msisdn, out var failure))
        {
            return failure!;
        }

        var digit = GetFormValue("Digits");
        var lang = digit switch
        {
            "1" => "en",
            "2" => "pidgin",
            "3" => "yo",
            "4" => "ig",
            "5" => "ha",
            _ => _options.DefaultLanguage
        };

        _session.SetLanguage(msisdn, lang);

        var res = new VoiceResponse();
        res.Redirect(new Uri("/api/ivr/menu", UriKind.Relative));
        return Twiml(res);
    }

    [HttpPost("menu")]
    public IActionResult Menu()
    {
        if (!TryGetMsisdn(out var msisdn, out var failure))
        {
            return failure!;
        }

        var lang = _session.GetLanguage(msisdn);

        var res = new VoiceResponse();
        var gather = new Gather(1, "/api/ivr/menu-action", "POST");
        Say(gather, T("Press 1 for balance. Press 2 for transfer.", lang), lang, "menu");
        res.Append(gather);
        res.Redirect(new Uri("/api/ivr/menu", UriKind.Relative));

        return Twiml(res);
    }

    [HttpPost("menu-action")]
    public IActionResult MenuAction()
    {
        var digit = GetFormValue("Digits");
        var res = new VoiceResponse();

        switch (digit)
        {
            case "1":
                res.Redirect(new Uri("/api/ivr/show-balance", UriKind.Relative));
                break;
            case "2":
                res.Redirect(new Uri("/api/ivr/enter-recipient", UriKind.Relative));
                break;
            default:
                res.Redirect(new Uri("/api/ivr/menu", UriKind.Relative));
                break;
        }

        return Twiml(res);
    }

    [HttpPost("show-balance")]
    public IActionResult ShowBalance()
    {
        if (!TryGetMsisdn(out var msisdn, out var failure))
        {
            return failure!;
        }

        var lang = _session.GetLanguage(msisdn);
        var balance = _banking.GetBalance(msisdn);

        var res = new VoiceResponse();
        var gather = new Gather(1, "/api/ivr/post-action", "POST");
        Say(gather, T($"Your balance is {balance:N0} naira. Press 1 for menu or 2 to end.", lang), lang);
        res.Append(gather);

        return Twiml(res);
    }

    [HttpPost("enter-recipient")]
    public IActionResult EnterRecipient()
    {
        if (!TryGetMsisdn(out var msisdn, out var failure))
        {
            return failure!;
        }

        var lang = _session.GetLanguage(msisdn);

        var res = new VoiceResponse();
        var gather = new Gather(10, "/api/ivr/confirm-recipient", "POST");
        Say(gather, T("Enter the 10 digit recipient account number.", lang), lang, "enter-recipient");
        res.Append(gather);
        res.Redirect(new Uri("/api/ivr/enter-recipient", UriKind.Relative));

        return Twiml(res);
    }

    [HttpPost("confirm-recipient")]
    public IActionResult ConfirmRecipient()
    {
        if (!TryGetMsisdn(out var msisdn, out var failure))
        {
            return failure!;
        }

        var lang = _session.GetLanguage(msisdn);
        var accountNumber = GetFormValue("Digits");

        if (!IsValidAccountNumber(accountNumber))
        {
            return PromptForRetry(
                T("The account number entered is invalid. Please enter a valid 10 digit account number.", lang),
                "/api/ivr/confirm-recipient",
                10,
                redirectPath: "/api/ivr/enter-recipient",
                language: lang,
                promptKey: "invalid-account");
        }

        var recipientName = _banking.ResolveRecipientName(accountNumber);

        _session.SetRecipient(msisdn, accountNumber, recipientName);

        var res = new VoiceResponse();
        var gather = new Gather(1, "/api/ivr/recipient-action", "POST");
        Say(gather, T($"Recipient is {recipientName}. Press 1 to continue or 2 to re-enter account number.", lang), lang);
        res.Append(gather);

        return Twiml(res);
    }

    [HttpPost("recipient-action")]
    public IActionResult RecipientAction()
    {
        var digit = GetFormValue("Digits");
        var res = new VoiceResponse();
        res.Redirect(new Uri(digit == "1" ? "/api/ivr/enter-amount" : "/api/ivr/enter-recipient", UriKind.Relative));
        return Twiml(res);
    }

    [HttpPost("enter-amount")]
    public IActionResult EnterAmount()
    {
        if (!TryGetMsisdn(out var msisdn, out var failure))
        {
            return failure!;
        }

        var lang = _session.GetLanguage(msisdn);

        var res = new VoiceResponse();
        var gather = new Gather(null, "/api/ivr/confirm-details", "POST", finishOnKey: "#");
        Say(gather, T("Enter transfer amount in naira, then press the hash key.", lang), lang, "enter-amount");
        res.Append(gather);
        res.Redirect(new Uri("/api/ivr/enter-amount", UriKind.Relative));

        return Twiml(res);
    }

    [HttpPost("confirm-details")]
    public IActionResult ConfirmDetails()
    {
        if (!TryGetMsisdn(out var msisdn, out var failure))
        {
            return failure!;
        }

        var lang = _session.GetLanguage(msisdn);
        var digits = GetFormValue("Digits");

        if (!decimal.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var amount) ||
            amount <= 0 ||
            amount > _options.MaximumTransferAmount)
        {
            return PromptForRetry(
                T($"Invalid amount. Enter an amount between 1 and {_options.MaximumTransferAmount:N0} naira, then press the hash key.", lang),
                "/api/ivr/confirm-details",
                null,
                "#",
                "/api/ivr/enter-amount",
                lang);
        }

        _session.SetAmount(msisdn, amount);

        var acc = _session.GetRecipient(msisdn);
        var name = _session.GetRecipientName(msisdn);
        var msg = T(
            $"You are about to send {amount:N0} naira to {name}, account number {acc}. Press 1 to continue or 2 to cancel.",
            lang);

        var gather = new Gather(1, "/api/ivr/transfer-pin", "POST");
        Say(gather, msg, lang);

        var res = new VoiceResponse();
        res.Append(gather);
        return Twiml(res);
    }

    [HttpPost("transfer-pin")]
    public IActionResult TransferPin()
    {
        if (!TryGetMsisdn(out var msisdn, out var failure))
        {
            return failure!;
        }

        var lang = _session.GetLanguage(msisdn);
        var digit = GetFormValue("Digits");
        var res = new VoiceResponse();

        if (digit == "1")
        {
            var gather = new Gather(4, "/api/ivr/execute-transfer", "POST");
            Say(gather, T("Enter your 4 digit transfer PIN.", lang), lang, "enter-pin");
            res.Append(gather);
        }
        else if (digit == "2")
        {
            var gather = new Gather(1, "/api/ivr/post-action", "POST");
            Say(gather, T("Transfer cancelled. Press 1 for menu or 2 to end.", lang), lang, "transfer-cancelled");
            res.Append(gather);
        }
        else
        {
            var gather = new Gather(1, "/api/ivr/transfer-pin", "POST");
            Say(gather, T("Invalid selection. Press 1 to continue or 2 to cancel.", lang), lang, "invalid-selection");
            res.Append(gather);
        }

        return Twiml(res);
    }

    [HttpPost("execute-transfer")]
    public IActionResult ExecuteTransfer()
    {
        if (!TryGetMsisdn(out var msisdn, out var failure))
        {
            return failure!;
        }

        var pin = GetFormValue("Digits");
        var lang = _session.GetLanguage(msisdn);

        var res = new VoiceResponse();
        var gather = new Gather(1, "/api/ivr/post-action", "POST");

        if (_banking.ValidateTransferPin(msisdn, pin))
        {
            var amount = _session.GetAmount(msisdn);
            var acc = _session.GetRecipient(msisdn);
            var name = _session.GetRecipientName(msisdn);

            var success = _banking.ExecuteTransfer(msisdn, acc, amount);
            var msg = success
                ? $"Transfer of {amount:N0} naira to {name} successful."
                : "Transfer failed due to insufficient balance.";

            Say(gather, T($"{msg} Press 1 for menu or 2 to end.", lang), lang);
        }
        else
        {
            Say(gather, T("Invalid PIN. Press 1 for menu or 2 to end.", lang), lang, "invalid-pin");
        }

        res.Append(gather);
        return Twiml(res);
    }

    [HttpPost("post-action")]
    public IActionResult PostAction()
    {
        TryGetMsisdn(out var msisdn, out _);
        var digit = GetFormValue("Digits");
        var res = new VoiceResponse();

        if (digit == "1")
        {
            res.Redirect(new Uri("/api/ivr/menu", UriKind.Relative));
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(msisdn))
            {
                _session.SetStatus(msisdn, false);
            }

            Say(res, "Thank you for using the banking service.", _options.DefaultLanguage, "thank-you");
            res.Hangup();
        }

        return Twiml(res);
    }

    private string GetMsisdn()
        => GetFormValue("MSISDN") is { Length: > 0 } msisdn
            ? msisdn
            : GetFormValue("From");

    private bool TryGetMsisdn(out string msisdn, out IActionResult? failure)
    {
        msisdn = GetMsisdn();
        failure = null;

        if (!string.IsNullOrWhiteSpace(msisdn))
        {
            return true;
        }

        if (_environment.IsDevelopment())
        {
            msisdn = "2340000000000";
            return true;
        }

        _logger.LogWarning("IVR request rejected because MSISDN/From was missing.");
        failure = ErrorResponse("Unable to process this call because the phone number is missing.", "missing-phone");
        return false;
    }

    private string GetFormValue(string key)
    {
        if (Request.HasFormContentType && Request.Form.TryGetValue(key, out var formValue))
        {
            return formValue.ToString();
        }

        return Request.Query.TryGetValue(key, out var queryValue) ? queryValue.ToString() : string.Empty;
    }

    private static bool IsValidAccountNumber(string accountNumber)
        => accountNumber.Length == 10 && accountNumber.All(char.IsDigit);

    private ContentResult ErrorResponse(string message, string? promptKey = null)
    {
        var response = new VoiceResponse();
        Say(response, message, _options.DefaultLanguage, promptKey);
        response.Hangup();
        return Twiml(response);
    }

    private ContentResult PromptForRetry(
        string message,
        string action,
        int? numDigits,
        string? finishOnKey = null,
        string? redirectPath = null,
        string? language = null,
        string? promptKey = null)
    {
        var response = new VoiceResponse();
        var gather = new Gather(numDigits, action, "POST", finishOnKey);
        Say(gather, message, language ?? _options.DefaultLanguage, promptKey);
        response.Append(gather);
        response.Redirect(new Uri(redirectPath ?? action, UriKind.Relative));
        return Twiml(response);
    }

    private ContentResult Twiml(VoiceResponse response)
        => Content(response.ToString(), "text/xml");
}
