namespace BuyItCommerce.Application.Orders.Contracts;

public sealed record OrderItemInput(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);
