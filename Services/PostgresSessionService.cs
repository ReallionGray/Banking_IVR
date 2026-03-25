using Banking_IVR.Persistence;
using Banking_IVR.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Banking_IVR.Services;

public class PostgresSessionService : ISessionService
{
    private readonly BankingIvrDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _sessionTimeout;
    private readonly string _defaultLanguage;

    public PostgresSessionService(
        BankingIvrDbContext dbContext,
        IMemoryCache cache,
        IOptions<IvrOptions> options)
    {
        _dbContext = dbContext;
        _cache = cache;
        _sessionTimeout = TimeSpan.FromMinutes(options.Value.SessionTimeoutMinutes);
        _defaultLanguage = options.Value.DefaultLanguage;
    }

    public void Initialize(string msisdn)
    {
        var existing = _dbContext.UssdSettings.SingleOrDefault(x => x.MSISDN == msisdn);
        if (existing is null)
        {
            _dbContext.UssdSettings.Add(new UssdSetting
            {
                Id = Guid.NewGuid(),
                MSISDN = msisdn,
                Language = _defaultLanguage,
                Status = true
            });
        }
        else
        {
            existing.Status = true;
            if (string.IsNullOrWhiteSpace(existing.Language))
            {
                existing.Language = _defaultLanguage;
            }
        }

        _dbContext.SaveChanges();
        TouchRuntimeSession(msisdn);
    }

    public bool Exists(string msisdn)
        => _dbContext.UssdSettings.AsNoTracking().Any(x => x.MSISDN == msisdn && x.Status);

    public string GetLanguage(string msisdn)
        => _dbContext.UssdSettings.AsNoTracking()
               .Where(x => x.MSISDN == msisdn)
               .Select(x => x.Language)
               .SingleOrDefault() ?? _defaultLanguage;

    public void SetLanguage(string msisdn, string language)
    {
        var setting = GetOrCreateSetting(msisdn);
        setting.Language = language;
        setting.Status = true;
        _dbContext.SaveChanges();
        TouchRuntimeSession(msisdn);
    }

    public bool GetStatus(string msisdn)
        => _dbContext.UssdSettings.AsNoTracking()
               .Where(x => x.MSISDN == msisdn)
               .Select(x => x.Status)
               .SingleOrDefault();

    public void SetStatus(string msisdn, bool status)
    {
        var setting = GetOrCreateSetting(msisdn);
        setting.Status = status;
        _dbContext.SaveChanges();

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
        var setting = _dbContext.UssdSettings.SingleOrDefault(x => x.MSISDN == msisdn);
        if (setting is not null)
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

        _dbContext.UssdSettings.Add(setting);
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

    private sealed class RuntimeSession
    {
        public string RecipientAccountNumber { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
