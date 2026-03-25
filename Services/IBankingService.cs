namespace Banking_IVR.Services;

public interface IBankingService
{
    decimal GetBalance(string callSid);
    string ResolveRecipientName(string accountNumber);
    bool ValidateTransferPin(string callSid, string pin);
    bool ExecuteTransfer(string callSid, string accountNumber, decimal amount);
}
