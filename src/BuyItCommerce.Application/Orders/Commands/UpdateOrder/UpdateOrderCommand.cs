using BuyItCommerce.Application.Orders.Contracts;
using MediatR;

namespace BuyItCommerce.Application.Orders.Commands.UpdateOrder;

public sealed record UpdateOrderCommand(
    Guid OrderId,
    Guid BuyerId,
    string BuyerName,
    IReadOnlyCollection<OrderItemInput> Items) : IRequest<OrderResponse>;
