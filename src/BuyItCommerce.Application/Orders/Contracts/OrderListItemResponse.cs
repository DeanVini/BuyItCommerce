namespace BuyItCommerce.Application.Orders.Contracts;

public sealed record OrderListItemResponse(
    Guid Id,
    Guid BuyerId,
    string BuyerName,
    string Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt);
