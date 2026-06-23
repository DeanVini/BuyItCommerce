using System.Text.Json;
using BuyItCommerce.Application.Abstractions.Outbox;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Exceptions;
using BuyItCommerce.Application.Orders.Contracts;
using MediatR;

namespace BuyItCommerce.Application.Orders.Commands.ProcessOrder;

public sealed class ProcessOrderCommandHandler(
    IOrderWriteRepository orders,
    IOutboxWriter outbox,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<ProcessOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(ProcessOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false)
            ?? throw new OrderNotFoundException(request.OrderId);

        var now = timeProvider.GetUtcNow();
        order.Process(now);
        orders.Update(order);

        var response = order.ToResponse();
        outbox.Add(OutboxMessage.Create(EOutboxEventType.OrderStatusChanged, JsonSerializer.Serialize(response), now));

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return response;
    }
}
