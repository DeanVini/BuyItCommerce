using BuyItCommerce.Domain.Exceptions;
using BuyItCommerce.Domain.Orders;
using FluentAssertions;

namespace BuyItCommerce.Tests.Domain;

public class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static Buyer ValidBuyer() => Buyer.Create(Guid.NewGuid(), "Comprador Teste");

    private static OrderItem ValidItem() =>
        OrderItem.Create(Guid.NewGuid(), "Produto", 10m, 2);

    private static Order InitiatedOrder() =>
        Order.Create(ValidBuyer(), [ValidItem()], Now);

    [Fact]
    public void Create_WithValidData_StartsAsIniciado()
    {
        var order = InitiatedOrder();

        order.Status.Should().Be(EOrderStatus.Created);
        order.Id.Should().NotBe(Guid.Empty);
        order.Items.Should().HaveCount(1);
        order.CreatedAt.Should().Be(Now);
        order.UpdatedAt.Should().Be(Now);
    }

    [Fact]
    public void Create_ComputesTotalAmount()
    {
        var buyer = ValidBuyer();
        var items = new[]
        {
            OrderItem.Create(Guid.NewGuid(), "A", 10m, 2),
            OrderItem.Create(Guid.NewGuid(), "B", 5m, 3)
        };

        var order = Order.Create(buyer, items, Now);

        order.TotalAmount.Should().Be(35m);
    }

    [Fact]
    public void Create_WithoutItems_Throws()
    {
        var act = () => Order.Create(ValidBuyer(), [], Now);

        act.Should().Throw<OrderMustHaveAtLeastOneItemException>();
    }

    [Fact]
    public void Create_WithNullBuyer_Throws()
    {
        var act = () => Order.Create(null!, [ValidItem()], Now);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullItems_Throws()
    {
        var act = () => Order.Create(ValidBuyer(), null!, Now);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Process_FromIniciado_MovesToProcessado()
    {
        var order = InitiatedOrder();

        order.Process(Now);

        order.Status.Should().Be(EOrderStatus.Processed);
    }

    [Fact]
    public void Process_WhenNotIniciado_Throws()
    {
        var order = InitiatedOrder();
        order.Process(Now);

        var act = () => order.Process(Now);

        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void Ship_FromProcessado_MovesToEnviado()
    {
        var order = InitiatedOrder();
        order.Process(Now);

        order.Ship(Now);

        order.Status.Should().Be(EOrderStatus.Shipped);
    }

    [Fact]
    public void Ship_FromIniciado_Throws()
    {
        var order = InitiatedOrder();

        var act = () => order.Ship(Now);

        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void Cancel_FromIniciado_MovesToCancelado()
    {
        var order = InitiatedOrder();

        order.Cancel(Now);

        order.Status.Should().Be(EOrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromProcessado_MovesToCancelado()
    {
        var order = InitiatedOrder();
        order.Process(Now);

        order.Cancel(Now);

        order.Status.Should().Be(EOrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromEnviado_Throws()
    {
        var order = InitiatedOrder();
        order.Process(Now);
        order.Ship(Now);

        var act = () => order.Cancel(Now);

        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelado_Throws()
    {
        var order = InitiatedOrder();
        order.Cancel(Now);

        var act = () => order.Cancel(Now);

        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void UpdateItems_WhenIniciado_ReplacesItems()
    {
        var order = InitiatedOrder();
        var newItems = new[] { OrderItem.Create(Guid.NewGuid(), "Novo", 20m, 1) };
        var later = Now.AddMinutes(5);

        order.UpdateItems(newItems, later);

        order.Items.Should().HaveCount(1);
        order.TotalAmount.Should().Be(20m);
        order.UpdatedAt.Should().Be(later);
    }

    [Fact]
    public void UpdateItems_WhenProcessado_Throws()
    {
        var order = InitiatedOrder();
        order.Process(Now);

        var act = () => order.UpdateItems([ValidItem()], Now);

        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void UpdateItems_WithEmptyCollection_Throws()
    {
        var order = InitiatedOrder();

        var act = () => order.UpdateItems([], Now);

        act.Should().Throw<OrderMustHaveAtLeastOneItemException>();
    }
}
