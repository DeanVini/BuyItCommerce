using BuyItCommerce.Application.Abstractions.Idempotency;
using BuyItCommerce.Application.Abstractions.Outbox;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Configuration;
using BuyItCommerce.Application.Orders.Commands.CreateOrder;
using BuyItCommerce.Application.Orders.Contracts;
using BuyItCommerce.Domain.Orders;
using BuyItCommerce.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace BuyItCommerce.Tests.Application;

public class CreateOrderCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static CreateOrderCommand ValidCommand() =>
        new(
            Guid.NewGuid(),
            "Comprador",
            [new OrderItemInput(Guid.NewGuid(), "Produto", 10m, 2)],
            "key-1");

    [Fact]
    public async Task Handle_PersistsOrderOutboxAndIdempotencyKey()
    {
        var orders = new Mock<IOrderWriteRepository>();
        var outbox = new Mock<IOutboxWriter>();
        var idempotency = new Mock<IIdempotencyStore>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var options = Options.Create(new IdempotencyOptions { KeyExpirationHours = 24 });

        var handler = new CreateOrderCommandHandler(
            orders.Object, outbox.Object, idempotency.Object, unitOfWork.Object, options, new FixedTimeProvider(Now));

        var response = await handler.Handle(ValidCommand(), CancellationToken.None);

        response.Status.Should().Be(nameof(EOrderStatus.Created));
        response.TotalAmount.Should().Be(20m);
        orders.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        outbox.Verify(o => o.Add(It.Is<OutboxMessage>(m => m.Type == EOutboxEventType.OrderCreated)), Times.Once);
        idempotency.Verify(s => s.Add(It.Is<IdempotencyRecord>(r => r.Key == "key-1")), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StoresResponsePayloadMatchingReturnedResponse()
    {
        var orders = new Mock<IOrderWriteRepository>();
        var outbox = new Mock<IOutboxWriter>();
        var idempotency = new Mock<IIdempotencyStore>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var options = Options.Create(new IdempotencyOptions());
        IdempotencyRecord? stored = null;
        idempotency.Setup(s => s.Add(It.IsAny<IdempotencyRecord>()))
            .Callback<IdempotencyRecord>(record => stored = record);

        var handler = new CreateOrderCommandHandler(
            orders.Object, outbox.Object, idempotency.Object, unitOfWork.Object, options, new FixedTimeProvider(Now));

        var response = await handler.Handle(ValidCommand(), CancellationToken.None);

        stored.Should().NotBeNull();
        stored!.ResponsePayload.Should().Contain(response.Id.ToString());
        stored.ExpiresAt.Should().Be(Now.AddHours(options.Value.KeyExpirationHours));
    }
}
