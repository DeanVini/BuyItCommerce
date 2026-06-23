namespace BuyItCommerce.Application.Orders.Contracts;

public sealed record OrderResponse(
    Guid Id,
    Guid BuyerId,
    string BuyerName,
    string Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<OrderItemResponse> Items);
