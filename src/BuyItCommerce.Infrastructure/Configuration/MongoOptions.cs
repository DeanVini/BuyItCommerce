using System.ComponentModel.DataAnnotations;

namespace BuyItCommerce.Infrastructure.Configuration;

public sealed class MongoOptions
{
    public const string SectionName = "Mongo";

    [Required]
    public string Database { get; set; } = "buyitcommerce";

    public string OrdersCollection { get; set; } = "orders_read";
}
