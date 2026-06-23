using System.Text.Json;
using BuyItCommerce.Application.Abstractions.Idempotency;
using BuyItCommerce.Application.Abstractions.Outbox;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Configuration;
using BuyItCommerce.Application.Orders.Contracts;
using BuyItCommerce.Domain.Orders;
using MediatR;
using Microsoft.Extensions.Options;

namespace BuyItCommerce.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(
    IOrderWriteRepository orders,
    IOutboxWriter outbox,
    IIdempotencyStore idempotencyStore,
    IUnitOfWork unitOfWork,
    IOptions<IdempotencyOptions> idempotencyOptions,
    TimeProvider timeProvider) : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var buyer = Buyer.Create(request.BuyerId, request.BuyerName);
        var order = Order.Create(buyer, request.Items.ToDomainItems(), now);

        await orders.AddAsync(order, cancellationToken).ConfigureAwait(false);

        var response = order.ToResponse();
        var payload = JsonSerializer.Serialize(response);

        outbox.Add(OutboxMessage.Create(EOutboxEventType.OrderCreated, payload, now));

        var expiresAt = now.AddHours(idempotencyOptions.Value.KeyExpirationHours);
        var hash = RequestHashGenerator.Compute(request);
        idempotencyStore.Add(new IdempotencyRecord(request.IdempotencyKey, hash, payload, now, expiresAt));

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return response;
    }
}
