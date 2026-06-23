using System.ComponentModel.DataAnnotations;

namespace BuyItCommerce.Application.Configuration;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    [Range(1, 3600)]
    public int ListingTtlSeconds { get; set; } = 30;
}
