using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BuyItCommerce.Infrastructure.ReadModel;

public sealed class OrderReadModel
{
    [BsonId]
    public Guid Id { get; set; }

    public Guid BuyerId { get; set; }

    public string BuyerName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalAmount { get; set; }

    [BsonRepresentation(BsonType.String)]
    public DateTimeOffset CreatedAt { get; set; }

    [BsonRepresentation(BsonType.String)]
    public DateTimeOffset UpdatedAt { get; set; }

    public List<OrderItemReadModel> Items { get; set; } = [];
}

public sealed class OrderItemReadModel
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal LineTotal { get; set; }
}
