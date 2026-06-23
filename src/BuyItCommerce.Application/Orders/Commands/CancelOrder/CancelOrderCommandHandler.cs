using System.Text.Json;
using BuyItCommerce.Application.Abstractions.Outbox;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Exceptions;
using BuyItCommerce.Application.Orders.Contracts;
using MediatR;

namespace BuyItCommerce.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler(
    IOrderWriteRepository orders,
    IOutboxWriter outbox,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<CancelOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false)
            ?? throw new OrderNotFoundException(request.OrderId);

        var now = timeProvider.GetUtcNow();
        order.Cancel(now);
        orders.Update(order);

        var response = order.ToResponse();
        outbox.Add(OutboxMessage.Create(EOutboxEventType.OrderStatusChanged, JsonSerializer.Serialize(response), now));

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return response;
    }
}
