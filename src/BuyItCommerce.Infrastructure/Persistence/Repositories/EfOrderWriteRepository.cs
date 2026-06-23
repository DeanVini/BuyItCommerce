using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace BuyItCommerce.Infrastructure.Persistence.Repositories;

internal sealed class EfOrderWriteRepository(OrdersDbContext context) : IOrderWriteRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Orders.FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken) =>
        await context.Orders.AddAsync(order, cancellationToken).ConfigureAwait(false);

    public void Update(Order order) => context.Orders.Update(order);
}
