using BuyItCommerce.Application.Orders.Contracts;

namespace BuyItCommerce.Api.Contracts;

public sealed record CreateOrderRequest(
    Guid BuyerId,
    string BuyerName,
    IReadOnlyCollection<OrderItemInput> Items);
