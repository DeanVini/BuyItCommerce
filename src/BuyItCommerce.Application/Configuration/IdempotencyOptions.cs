using System.ComponentModel.DataAnnotations;

namespace BuyItCommerce.Application.Configuration;

public sealed class IdempotencyOptions
{
    public const string SectionName = "Idempotency";

    [Range(1, 8760)]
    public int KeyExpirationHours { get; set; } = 24;
}
