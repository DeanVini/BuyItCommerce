using BuyItCommerce.Application.Abstractions.Idempotency;
using BuyItCommerce.Application.Abstractions.Outbox;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Domain.Orders;
using BuyItCommerce.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace BuyItCommerce.Infrastructure.Persistence;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options, IOutboxSignal outboxSignal)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<IdempotencyRecord> IdempotencyKeys => Set<IdempotencyRecord>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var pendingMessageIds = ChangeTracker
            .Entries<OutboxMessage>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity.Id)
            .ToList();

        var affected = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var messageId in pendingMessageIds)
        {
            outboxSignal.Notify(messageId);
        }

        return affected;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        SaveChangesAsync(cancellationToken);
}
