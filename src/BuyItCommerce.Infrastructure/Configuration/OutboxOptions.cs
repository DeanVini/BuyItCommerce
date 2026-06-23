using System.ComponentModel.DataAnnotations;

namespace BuyItCommerce.Infrastructure.Configuration;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    [Range(1, 3600)]
    public int PollingIntervalSeconds { get; set; } = 5;
}
