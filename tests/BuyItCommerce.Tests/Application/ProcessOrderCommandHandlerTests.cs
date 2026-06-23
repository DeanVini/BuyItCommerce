using BuyItCommerce.Application.Abstractions.Outbox;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Exceptions;
using BuyItCommerce.Application.Orders.Commands.ProcessOrder;
using BuyItCommerce.Domain.Orders;
using BuyItCommerce.Tests.Support;
using FluentAssertions;
using Moq;

namespace BuyItCommerce.Tests.Application;

public class ProcessOrderCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static Order CreatedOrder()
    {
        var buyer = Buyer.Create(Guid.NewGuid(), "Comprador");
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Produto", 10m, 1) };
        return Order.Create(buyer, items, Now);
    }

    [Fact]
    public async Task Handle_WhenOrderExists_ProcessesAndSaves()
    {
        var order = CreatedOrder();
        var orders = new Mock<IOrderWriteRepository>();
        orders.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        var outbox = new Mock<IOutboxWriter>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new ProcessOrderCommandHandler(
            orders.Object, outbox.Object, unitOfWork.Object, new FixedTimeProvider(Now));

        var response = await handler.Handle(new ProcessOrderCommand(order.Id), CancellationToken.None);

        response.Status.Should().Be(nameof(EOrderStatus.Processed));
        orders.Verify(r => r.Update(order), Times.Once);
        outbox.Verify(o => o.Add(It.Is<OutboxMessage>(m => m.Type == EOutboxEventType.OrderStatusChanged)), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderMissing_ThrowsOrderNotFound()
    {
        var orders = new Mock<IOrderWriteRepository>();
        orders.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var handler = new ProcessOrderCommandHandler(
            orders.Object, Mock.Of<IOutboxWriter>(), Mock.Of<IUnitOfWork>(), new FixedTimeProvider(Now));

        var act = async () => await handler.Handle(new ProcessOrderCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }
}
