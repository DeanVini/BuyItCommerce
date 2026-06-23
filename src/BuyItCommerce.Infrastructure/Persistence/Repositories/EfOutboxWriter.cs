using BuyItCommerce.Application.Abstractions.Outbox;

namespace BuyItCommerce.Infrastructure.Persistence.Repositories;

internal sealed class EfOutboxWriter(OrdersDbContext context) : IOutboxWriter
{
    public void Add(OutboxMessage message) => context.OutboxMessages.Add(message);
}
