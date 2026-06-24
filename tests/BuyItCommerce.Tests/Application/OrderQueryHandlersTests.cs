using BuyItCommerce.Application.Abstractions.Caching;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Configuration;
using BuyItCommerce.Application.Orders.Contracts;
using BuyItCommerce.Application.Orders.Queries.GetOrderById;
using BuyItCommerce.Application.Orders.Queries.ListOrders;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace BuyItCommerce.Tests.Application;

public class OrderQueryHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetOrderById_ReturnsRepositoryResultThroughCache()
    {
        var id = Guid.NewGuid();
        var expected = new OrderResponse(id, Guid.NewGuid(), "Comprador", "Created", 10m, Now, Now, []);

        var repository = new Mock<IOrderReadRepository>();
        repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, Task<OrderResponse?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns((string _, Func<CancellationToken, Task<OrderResponse?>> factory, TimeSpan _, CancellationToken ct) => factory(ct));

        var handler = new GetOrderByIdQueryHandler(repository.Object, cache.Object, Options.Create(new CacheOptions()));

        var result = await handler.Handle(new GetOrderByIdQuery(id), CancellationToken.None);

        result.Should().Be(expected);
        repository.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListOrders_ReturnsPagedResultThroughCache()
    {
        var expected = new PagedResult<OrderListItemResponse>([], 1, 20, 0);

        var repository = new Mock<IOrderReadRepository>();
        repository.Setup(r => r.ListAsync(It.IsAny<OrderListFilter>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, Task<PagedResult<OrderListItemResponse>>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns((string _, Func<CancellationToken, Task<PagedResult<OrderListItemResponse>>> factory, TimeSpan _, CancellationToken ct) => factory(ct));

        var handler = new ListOrdersQueryHandler(repository.Object, cache.Object, Options.Create(new CacheOptions()));

        var result = await handler.Handle(new ListOrdersQuery(null, null), CancellationToken.None);

        result.Should().BeSameAs(expected);
        repository.Verify(r => r.ListAsync(It.IsAny<OrderListFilter>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
