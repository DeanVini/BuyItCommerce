using BuyItCommerce.Application.Orders.Contracts;
using MediatR;

namespace BuyItCommerce.Application.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : IRequest<OrderResponse>;
