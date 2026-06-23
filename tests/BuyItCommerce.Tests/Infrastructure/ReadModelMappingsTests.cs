using BuyItCommerce.Application.Orders.Contracts;
using BuyItCommerce.Infrastructure.ReadModel;
using FluentAssertions;

namespace BuyItCommerce.Tests.Infrastructure;

public class ReadModelMappingsTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ToReadModel_MapsAllFields()
    {
        var response = new OrderResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Comprador",
            "Created",
            20m,
            Now,
            Now,
            [new OrderItemResponse(Guid.NewGuid(), "Produto", 10m, 2, 20m)]);

        var model = response.ToReadModel();

        model.Id.Should().Be(response.Id);
        model.BuyerId.Should().Be(response.BuyerId);
        model.BuyerName.Should().Be("Comprador");
        model.Status.Should().Be("Created");
        model.TotalAmount.Should().Be(20m);
        model.CreatedAt.Should().Be(Now);
        model.Items.Should().HaveCount(1);
        model.Items[0].ProductName.Should().Be("Produto");
        model.Items[0].LineTotal.Should().Be(20m);
    }
}
