using BuyItCommerce.Application.Orders.Commands.CreateOrder;
using BuyItCommerce.Application.Orders.Contracts;
using FluentValidation.TestHelper;

namespace BuyItCommerce.Tests.Application;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    private static CreateOrderCommand Valid() =>
        new(
            Guid.NewGuid(),
            "Comprador",
            [new OrderItemInput(Guid.NewGuid(), "Produto", 10m, 1)],
            "key-1");

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithoutItems_HasError()
    {
        var command = Valid() with { Items = [] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Items);
    }

    [Fact]
    public void Validate_WithoutIdempotencyKey_HasError()
    {
        var command = Valid() with { IdempotencyKey = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.IdempotencyKey);
    }

    [Fact]
    public void Validate_WithNonPositivePrice_HasError()
    {
        var command = Valid() with { Items = [new OrderItemInput(Guid.NewGuid(), "Produto", 0m, 1)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }
}
