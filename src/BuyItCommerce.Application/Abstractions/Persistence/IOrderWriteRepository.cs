using BuyItCommerce.Domain.Orders;

namespace BuyItCommerce.Application.Abstractions.Persistence;

public interface IOrderWriteRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Order order, CancellationToken cancellationToken);

    void Update(Order order);
}
