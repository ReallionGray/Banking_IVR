using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Banking_IVR.Services;

public class BankingService : IBankingService
{
    private static readonly Dictionary<string, string> Recipients = new()
    {
        ["0123456789"] = "Adaobi Okafor",
        ["1234567890"] = "Bala Musa",
        ["2345678901"] = "Tunde Adebayo"
    };

    private readonly ConcurrentDictionary<string, decimal> _balances = new();
    private readonly ConcurrentDictionary<string, string> _pins = new();
    private readonly decimal _initialBalance;
    private readonly Lock _lock = new();

    public BankingService(IOptions<IvrOptions> options)
    {
        _initialBalance = options.Value.InitialBalance;
    }

    public decimal GetBalance(string callSid)
        => _balances.GetOrAdd(callSid, _initialBalance);

    public string ResolveRecipientName(string accountNumber)
        => Recipients.TryGetValue(accountNumber, out var name) ? name : "Valued Customer";

    public bool ValidateTransferPin(string callSid, string pin)
        => _pins.GetOrAdd(callSid, "1234") == pin;

    public bool ExecuteTransfer(string callSid, string accountNumber, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(accountNumber) || amount <= 0)
        {
            return false;
        }

        lock (_lock)
        {
            var currentBalance = GetBalance(callSid);
            if (amount <= 0 || currentBalance < amount)
            {
                return false;
            }

            _balances[callSid] = currentBalance - amount;
            return true;
        }
    }
}
