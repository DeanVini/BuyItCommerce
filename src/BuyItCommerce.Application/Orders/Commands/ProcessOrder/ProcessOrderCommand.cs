using BuyItCommerce.Application.Orders.Contracts;
using MediatR;

namespace BuyItCommerce.Application.Orders.Commands.ProcessOrder;

public sealed record ProcessOrderCommand(Guid OrderId) : IRequest<OrderResponse>;
