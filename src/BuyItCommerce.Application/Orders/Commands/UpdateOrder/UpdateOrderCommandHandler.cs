using System.Text.Json;
using BuyItCommerce.Application.Abstractions.Outbox;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Exceptions;
using BuyItCommerce.Application.Orders.Contracts;
using BuyItCommerce.Domain.Orders;
using MediatR;

namespace BuyItCommerce.Application.Orders.Commands.UpdateOrder;

public sealed class UpdateOrderCommandHandler(
    IOrderWriteRepository orders,
    IOutboxWriter outbox,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<UpdateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false)
            ?? throw new OrderNotFoundException(request.OrderId);

        var now = timeProvider.GetUtcNow();

        order.ChangeBuyer(Buyer.Create(request.BuyerId, request.BuyerName), now);
        order.UpdateItems(request.Items.ToDomainItems(), now);
        orders.Update(order);

        var response = order.ToResponse();
        outbox.Add(OutboxMessage.Create(EOutboxEventType.OrderStatusChanged, JsonSerializer.Serialize(response), now));

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return response;
    }
}
