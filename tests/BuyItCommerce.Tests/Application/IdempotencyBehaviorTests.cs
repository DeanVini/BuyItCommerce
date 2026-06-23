using System.Text.Json;
using BuyItCommerce.Application.Abstractions.Idempotency;
using BuyItCommerce.Application.Behaviors;
using BuyItCommerce.Application.Exceptions;
using BuyItCommerce.Application.Orders.Commands.CreateOrder;
using BuyItCommerce.Application.Orders.Commands.ProcessOrder;
using BuyItCommerce.Application.Orders.Contracts;
using FluentAssertions;
using MediatR;
using Moq;

namespace BuyItCommerce.Tests.Application;

public class IdempotencyBehaviorTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static CreateOrderCommand Command(string key) =>
        new(Guid.NewGuid(), "Comprador", [new OrderItemInput(Guid.NewGuid(), "Produto", 10m, 1)], key);

    private static OrderResponse SampleResponse() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Comprador", "Created", 10m, Now, Now, []);

    [Fact]
    public async Task Handle_WhenKeyExistsWithSameHash_ReplaysStoredResponse()
    {
        var command = Command("key-1");
        var response = SampleResponse();
        var record = new IdempotencyRecord(
            "key-1",
            RequestHashGenerator.Compute(command),
            JsonSerializer.Serialize(response),
            Now,
            Now);
        var store = new Mock<IIdempotencyStore>();
        store.Setup(s => s.FindAsync("key-1", It.IsAny<CancellationToken>())).ReturnsAsync(record);
        var behavior = new IdempotencyBehavior<CreateOrderCommand, OrderResponse>(store.Object);

        var nextCalled = false;
        RequestHandlerDelegate<OrderResponse> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(SampleResponse());
        };

        var result = await behavior.Handle(command, next, CancellationToken.None);

        nextCalled.Should().BeFalse();
        result.Id.Should().Be(response.Id);
    }

    [Fact]
    public async Task Handle_WhenKeyExistsWithDifferentHash_Throws()
    {
        var command = Command("key-1");
        var record = new IdempotencyRecord("key-1", "hash-diferente", JsonSerializer.Serialize(SampleResponse()), Now, Now);
        var store = new Mock<IIdempotencyStore>();
        store.Setup(s => s.FindAsync("key-1", It.IsAny<CancellationToken>())).ReturnsAsync(record);
        var behavior = new IdempotencyBehavior<CreateOrderCommand, OrderResponse>(store.Object);

        RequestHandlerDelegate<OrderResponse> next = () => Task.FromResult(SampleResponse());

        var act = async () => await behavior.Handle(command, next, CancellationToken.None);

        await act.Should().ThrowAsync<IdempotencyConflictException>();
    }

    [Fact]
    public async Task Handle_WhenKeyNotFound_CallsNext()
    {
        var command = Command("key-1");
        var store = new Mock<IIdempotencyStore>();
        store.Setup(s => s.FindAsync("key-1", It.IsAny<CancellationToken>())).ReturnsAsync((IdempotencyRecord?)null);
        var behavior = new IdempotencyBehavior<CreateOrderCommand, OrderResponse>(store.Object);

        var expected = SampleResponse();
        RequestHandlerDelegate<OrderResponse> next = () => Task.FromResult(expected);

        var result = await behavior.Handle(command, next, CancellationToken.None);

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task Handle_WhenRequestNotIdempotent_DoesNotTouchStore()
    {
        var store = new Mock<IIdempotencyStore>();
        var behavior = new IdempotencyBehavior<ProcessOrderCommand, OrderResponse>(store.Object);

        var expected = SampleResponse();
        RequestHandlerDelegate<OrderResponse> next = () => Task.FromResult(expected);

        var result = await behavior.Handle(new ProcessOrderCommand(Guid.NewGuid()), next, CancellationToken.None);

        result.Should().BeSameAs(expected);
        store.Verify(s => s.FindAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
