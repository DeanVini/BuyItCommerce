using BuyItCommerce.Application.Orders.Commands.UpdateOrder;
using BuyItCommerce.Application.Orders.Contracts;
using BuyItCommerce.Application.Orders.Queries.ListOrders;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BuyItCommerce.Tests.Application;

public class OrderValidatorsTests
{
    private readonly UpdateOrderCommandValidator _updateValidator = new();
    private readonly ListOrdersQueryValidator _listValidator = new();

    [Fact]
    public void UpdateOrder_WithoutItems_HasError()
    {
        var command = new UpdateOrderCommand(Guid.NewGuid(), Guid.NewGuid(), "Comprador", []);

        var result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Items);
    }

    [Fact]
    public void UpdateOrder_WithValidData_HasNoErrors()
    {
        var command = new UpdateOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Comprador",
            [new OrderItemInput(Guid.NewGuid(), "Produto", 10m, 1)]);

        var result = _updateValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void ListOrders_WithInvalidPaging_HasError(int page, int pageSize)
    {
        var query = new ListOrdersQuery(null, null, page, pageSize);

        var result = _listValidator.TestValidate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ListOrders_WithValidPaging_HasNoErrors()
    {
        var query = new ListOrdersQuery(null, null, 1, 20);

        var result = _listValidator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
