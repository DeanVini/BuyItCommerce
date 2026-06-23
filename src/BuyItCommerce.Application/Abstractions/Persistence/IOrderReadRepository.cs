using BuyItCommerce.Application.Orders.Contracts;

namespace BuyItCommerce.Application.Abstractions.Persistence;

public interface IOrderReadRepository
{
    Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<OrderListItemResponse>> ListAsync(OrderListFilter filter, CancellationToken cancellationToken);
}
