namespace Banking_IVR.Services;

public interface ISessionService
{
    void Initialize(string msisdn);
    bool Exists(string msisdn);
    string GetLanguage(string msisdn);
    void SetLanguage(string msisdn, string language);
    bool TryGetCachedLanguage(string msisdn, out string language);
    bool GetStatus(string msisdn);
    void SetStatus(string msisdn, bool status);
    void SetRecipient(string msisdn, string accountNumber, string recipientName);
    string GetRecipient(string msisdn);
    string GetRecipientName(string msisdn);
    void SetAmount(string msisdn, decimal amount);
    decimal GetAmount(string msisdn);
}
