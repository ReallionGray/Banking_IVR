using System.ComponentModel.DataAnnotations;

namespace Banking_IVR.Services;

public sealed class IvrOptions
{
    public const string SectionName = "Ivr";

    [Required]
    public string PersistenceMode { get; init; } = "InMemory";

    [Range(1, 120)]
    public int SessionTimeoutMinutes { get; init; } = 20;

    [Range(typeof(decimal), "1", "1000000000")]
    public decimal InitialBalance { get; init; } = 250000m;

    [Range(typeof(decimal), "1", "1000000000")]
    public decimal MaximumTransferAmount { get; init; } = 1000000m;

    [Required]
    public string DefaultLanguage { get; init; } = "en";

    [Required]
    public string AudioBasePath { get; init; } = "/audio";

    public string? PublicBaseUrl { get; init; }
}
