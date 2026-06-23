using BuyItCommerce.Application.Orders.Contracts;
using MediatR;

namespace BuyItCommerce.Application.Orders.Commands.ShipOrder;

public sealed record ShipOrderCommand(Guid OrderId) : IRequest<OrderResponse>;
