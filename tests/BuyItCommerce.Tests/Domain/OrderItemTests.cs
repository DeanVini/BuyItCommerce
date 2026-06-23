using BuyItCommerce.Domain.Exceptions;
using BuyItCommerce.Domain.Orders;
using FluentAssertions;

namespace BuyItCommerce.Tests.Domain;

public class OrderItemTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var productId = Guid.NewGuid();

        var item = OrderItem.Create(productId, "Produto", 12.5m, 4);

        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("Produto");
        item.UnitPrice.Should().Be(12.5m);
        item.Quantity.Should().Be(4);
        item.LineTotal.Should().Be(50m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositivePrice_Throws(decimal price)
    {
        var act = () => OrderItem.Create(Guid.NewGuid(), "Produto", price, 1);

        act.Should().Throw<OrderItemPriceMustBePositiveException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Create_WithNonPositiveQuantity_Throws(int quantity)
    {
        var act = () => OrderItem.Create(Guid.NewGuid(), "Produto", 10m, quantity);

        act.Should().Throw<OrderItemQuantityMustBePositiveException>();
    }
}
