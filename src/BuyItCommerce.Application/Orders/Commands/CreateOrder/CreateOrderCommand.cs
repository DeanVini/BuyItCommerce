using BuyItCommerce.Application.Abstractions.Idempotency;
using BuyItCommerce.Application.Orders.Contracts;
using MediatR;

namespace BuyItCommerce.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid BuyerId,
    string BuyerName,
    IReadOnlyCollection<OrderItemInput> Items,
    string IdempotencyKey) : IRequest<OrderResponse>, IIdempotentRequest;
