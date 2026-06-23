using BuyItCommerce.Application.Abstractions.Idempotency;
using Microsoft.EntityFrameworkCore;

namespace BuyItCommerce.Infrastructure.Persistence.Repositories;

internal sealed class EfIdempotencyStore(OrdersDbContext context) : IIdempotencyStore
{
    public Task<IdempotencyRecord?> FindAsync(string key, CancellationToken cancellationToken) =>
        context.IdempotencyKeys.AsNoTracking().FirstOrDefaultAsync(record => record.Key == key, cancellationToken);

    public void Add(IdempotencyRecord record) => context.IdempotencyKeys.Add(record);
}
