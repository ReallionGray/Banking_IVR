namespace Banking_IVR.Persistence.Entities;

public class UssdSetting
{
    public Guid Id { get; set; }
    public string MSISDN { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public bool Status { get; set; }
}
