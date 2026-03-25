using Banking_IVR.Persistence.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Banking_IVR.Services;

public class InMemorySessionService : ISessionService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _sessionTimeout;
    private readonly TimeSpan _languagePreferenceCacheTimeout;
    private readonly bool _enableLanguagePreferenceCache;
    private readonly string _defaultLanguage;
    private readonly Dictionary<string, UssdSetting> _settings;
    private readonly Lock _lock = new();

    public InMemorySessionService(IMemoryCache cache, IOptions<IvrOptions> options)
    {
        _cache = cache;
        _sessionTimeout = TimeSpan.FromMinutes(options.Value.SessionTimeoutMinutes);
        _languagePreferenceCacheTimeout = TimeSpan.FromMinutes(options.Value.LanguagePreferenceCacheMinutes);
        _enableLanguagePreferenceCache = options.Value.EnableLanguagePreferenceCache;
        _defaultLanguage = options.Value.DefaultLanguage;
        _settings = new Dictionary<string, UssdSetting>(StringComparer.Ordinal)
        {
            ["2348012345678"] = new()
            {
                Id = Guid.NewGuid(),
                MSISDN = "2348012345678",
                Language = "en",
                Status = true
            },
            ["2348098765432"] = new()
            {
                Id = Guid.NewGuid(),
                MSISDN = "2348098765432",
                Language = "yo",
                Status = true
            },
            ["2348077700011"] = new()
            {
                Id = Guid.NewGuid(),
                MSISDN = "2348077700011",
                Language = "pidgin",
                Status = false
            }
        };
    }

    public void Initialize(string msisdn)
    {
        lock (_lock)
        {
            if (!_settings.TryGetValue(msisdn, out var setting))
            {
                setting = new UssdSetting
                {
                    Id = Guid.NewGuid(),
                    MSISDN = msisdn,
                    Language = _defaultLanguage,
                    Status = true
                };
                _settings[msisdn] = setting;
            }
            else
            {
                setting.Status = true;
                if (string.IsNullOrWhiteSpace(setting.Language))
                {
                    setting.Language = _defaultLanguage;
                }
            }
        }

        TouchRuntimeSession(msisdn);
    }

    public bool Exists(string msisdn)
    {
        lock (_lock)
        {
            return _settings.TryGetValue(msisdn, out var setting) && setting.Status;
        }
    }

    public string GetLanguage(string msisdn)
    {
        lock (_lock)
        {
            return _settings.TryGetValue(msisdn, out var setting) && !string.IsNullOrWhiteSpace(setting.Language)
                ? setting.Language
                : _defaultLanguage;
        }
    }

    public void SetLanguage(string msisdn, string language)
    {
        lock (_lock)
        {
            var setting = GetOrCreateSetting(msisdn);
            setting.Language = language;
            setting.Status = true;
        }

        CacheLanguage(msisdn, language);
        TouchRuntimeSession(msisdn);
    }

    public bool TryGetCachedLanguage(string msisdn, out string language)
    {
        if (!_enableLanguagePreferenceCache)
        {
            language = string.Empty;
            return false;
        }

        if (_cache.TryGetValue(GetLanguagePreferenceCacheKey(msisdn), out string? cachedLanguage) &&
            !string.IsNullOrWhiteSpace(cachedLanguage))
        {
            language = cachedLanguage;
            return true;
        }

        language = string.Empty;
        return false;
    }

    public bool GetStatus(string msisdn)
    {
        lock (_lock)
        {
            return _settings.TryGetValue(msisdn, out var setting) && setting.Status;
        }
    }

    public void SetStatus(string msisdn, bool status)
    {
        lock (_lock)
        {
            var setting = GetOrCreateSetting(msisdn);
            setting.Status = status;
        }

        if (status)
        {
            TouchRuntimeSession(msisdn);
        }
        else
        {
            _cache.Remove(msisdn);
        }
    }

    public void SetRecipient(string msisdn, string accountNumber, string recipientName)
    {
        var session = GetRuntimeSession(msisdn);
        session.RecipientAccountNumber = accountNumber;
        session.RecipientName = recipientName;
    }

    public string GetRecipient(string msisdn)
        => GetRuntimeSession(msisdn).RecipientAccountNumber;

    public string GetRecipientName(string msisdn)
        => GetRuntimeSession(msisdn).RecipientName;

    public void SetAmount(string msisdn, decimal amount)
        => GetRuntimeSession(msisdn).Amount = amount;

    public decimal GetAmount(string msisdn)
        => GetRuntimeSession(msisdn).Amount;

    private UssdSetting GetOrCreateSetting(string msisdn)
    {
        if (_settings.TryGetValue(msisdn, out var setting))
        {
            return setting;
        }

        setting = new UssdSetting
        {
            Id = Guid.NewGuid(),
            MSISDN = msisdn,
            Language = _defaultLanguage,
            Status = true
        };
        _settings[msisdn] = setting;
        return setting;
    }

    private RuntimeSession GetRuntimeSession(string msisdn)
        => _cache.GetOrCreate(msisdn, entry =>
        {
            entry.SetSlidingExpiration(_sessionTimeout);
            return new RuntimeSession();
        })!;

    private void TouchRuntimeSession(string msisdn)
        => _ = GetRuntimeSession(msisdn);

    private void CacheLanguage(string msisdn, string language)
    {
        if (!_enableLanguagePreferenceCache)
        {
            return;
        }

        _cache.Set(GetLanguagePreferenceCacheKey(msisdn), language, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _languagePreferenceCacheTimeout
        });
    }

    private static string GetLanguagePreferenceCacheKey(string msisdn)
        => $"langpref:{msisdn}";

    private sealed class RuntimeSession
    {
        public string RecipientAccountNumber { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
