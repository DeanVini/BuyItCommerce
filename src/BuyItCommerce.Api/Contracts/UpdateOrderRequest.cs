using BuyItCommerce.Application.Orders.Contracts;

namespace BuyItCommerce.Api.Contracts;

public sealed record UpdateOrderRequest(
    Guid BuyerId,
    string BuyerName,
    IReadOnlyCollection<OrderItemInput> Items);
