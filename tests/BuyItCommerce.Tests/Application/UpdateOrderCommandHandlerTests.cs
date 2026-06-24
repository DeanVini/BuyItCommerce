using BuyItCommerce.Application.Abstractions.Outbox;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Exceptions;
using BuyItCommerce.Application.Orders.Commands.UpdateOrder;
using BuyItCommerce.Application.Orders.Contracts;
using BuyItCommerce.Domain.Orders;
using BuyItCommerce.Tests.Support;
using FluentAssertions;
using Moq;

namespace BuyItCommerce.Tests.Application;

public class UpdateOrderCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static Order CreatedOrder() =>
        Order.Create(
            Buyer.Create(Guid.NewGuid(), "Comprador"),
            [OrderItem.Create(Guid.NewGuid(), "Produto", 10m, 1)],
            Now);

    [Fact]
    public async Task Handle_WhenOrderCreated_UpdatesItemsAndBuyer()
    {
        var order = CreatedOrder();
        var orders = new Mock<IOrderWriteRepository>();
        orders.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        var outbox = new Mock<IOutboxWriter>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateOrderCommandHandler(orders.Object, outbox.Object, unitOfWork.Object, new FixedTimeProvider(Now));

        var command = new UpdateOrderCommand(
            order.Id,
            Guid.NewGuid(),
            "Novo Comprador",
            [new OrderItemInput(Guid.NewGuid(), "Outro", 30m, 2)]);

        var response = await handler.Handle(command, CancellationToken.None);

        response.BuyerName.Should().Be("Novo Comprador");
        response.TotalAmount.Should().Be(60m);
        response.Items.Should().HaveCount(1);
        orders.Verify(r => r.Update(order), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderMissing_ThrowsOrderNotFound()
    {
        var orders = new Mock<IOrderWriteRepository>();
        orders.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var handler = new UpdateOrderCommandHandler(
            orders.Object, Mock.Of<IOutboxWriter>(), Mock.Of<IUnitOfWork>(), new FixedTimeProvider(Now));

        var command = new UpdateOrderCommand(Guid.NewGuid(), Guid.NewGuid(), "X", [new OrderItemInput(Guid.NewGuid(), "P", 1m, 1)]);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }
}
