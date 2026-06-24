using BuyItCommerce.Application.Abstractions.Outbox;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Exceptions;
using BuyItCommerce.Application.Orders.Commands.ShipOrder;
using BuyItCommerce.Domain.Orders;
using BuyItCommerce.Tests.Support;
using FluentAssertions;
using Moq;

namespace BuyItCommerce.Tests.Application;

public class ShipOrderCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static Order ProcessedOrder()
    {
        var order = Order.Create(
            Buyer.Create(Guid.NewGuid(), "Comprador"),
            [OrderItem.Create(Guid.NewGuid(), "Produto", 10m, 1)],
            Now);
        order.Process(Now);
        return order;
    }

    [Fact]
    public async Task Handle_WhenOrderProcessed_ShipsAndSaves()
    {
        var order = ProcessedOrder();
        var orders = new Mock<IOrderWriteRepository>();
        orders.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        var outbox = new Mock<IOutboxWriter>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new ShipOrderCommandHandler(orders.Object, outbox.Object, unitOfWork.Object, new FixedTimeProvider(Now));

        var response = await handler.Handle(new ShipOrderCommand(order.Id), CancellationToken.None);

        response.Status.Should().Be(nameof(EOrderStatus.Shipped));
        outbox.Verify(o => o.Add(It.Is<OutboxMessage>(m => m.Type == EOutboxEventType.OrderStatusChanged)), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderMissing_ThrowsOrderNotFound()
    {
        var orders = new Mock<IOrderWriteRepository>();
        orders.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var handler = new ShipOrderCommandHandler(
            orders.Object, Mock.Of<IOutboxWriter>(), Mock.Of<IUnitOfWork>(), new FixedTimeProvider(Now));

        var act = async () => await handler.Handle(new ShipOrderCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }
}
