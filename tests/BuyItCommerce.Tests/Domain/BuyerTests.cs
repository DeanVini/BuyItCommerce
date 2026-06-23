using BuyItCommerce.Domain.Exceptions;
using BuyItCommerce.Domain.Orders;
using FluentAssertions;

namespace BuyItCommerce.Tests.Domain;

public class BuyerTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var id = Guid.NewGuid();

        var buyer = Buyer.Create(id, "  Comprador  ");

        buyer.Id.Should().Be(id);
        buyer.Name.Should().Be("Comprador");
    }

    [Fact]
    public void Create_WithEmptyId_Throws()
    {
        var act = () => Buyer.Create(Guid.Empty, "Comprador");

        act.Should().Throw<BuyerRequiredException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_Throws(string name)
    {
        var act = () => Buyer.Create(Guid.NewGuid(), name);

        act.Should().Throw<BuyerRequiredException>();
    }
}
