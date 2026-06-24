using BuyItCommerce.Application.Behaviors;
using BuyItCommerce.Application.Orders.Commands.ProcessOrder;
using BuyItCommerce.Application.Orders.Contracts;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace BuyItCommerce.Tests.Application;

public class ValidationBehaviorTests
{
    private static OrderResponse Sample() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Comprador", "Created", 0m, DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, []);

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        var validator = new Mock<IValidator<ProcessOrderCommand>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ProcessOrderCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("OrderId", "obrigatório")]));

        var behavior = new ValidationBehavior<ProcessOrderCommand, OrderResponse>([validator.Object]);
        RequestHandlerDelegate<OrderResponse> next = () => Task.FromResult(Sample());

        var act = async () => await behavior.Handle(new ProcessOrderCommand(Guid.NewGuid()), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenValid_CallsNext()
    {
        var validator = new Mock<IValidator<ProcessOrderCommand>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ProcessOrderCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<ProcessOrderCommand, OrderResponse>([validator.Object]);
        var expected = Sample();
        RequestHandlerDelegate<OrderResponse> next = () => Task.FromResult(expected);

        var result = await behavior.Handle(new ProcessOrderCommand(Guid.NewGuid()), next, CancellationToken.None);

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task Handle_WhenNoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<ProcessOrderCommand, OrderResponse>([]);
        var expected = Sample();
        RequestHandlerDelegate<OrderResponse> next = () => Task.FromResult(expected);

        var result = await behavior.Handle(new ProcessOrderCommand(Guid.NewGuid()), next, CancellationToken.None);

        result.Should().BeSameAs(expected);
    }
}
