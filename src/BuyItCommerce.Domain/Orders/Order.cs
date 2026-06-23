using BuyItCommerce.Domain.Exceptions;

namespace BuyItCommerce.Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }

    public Buyer Buyer { get; private set; } = null!;

    public EOrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(item => item.LineTotal);

    private Order()
    {
    }

    private Order(Guid id, Buyer buyer, IEnumerable<OrderItem> items, DateTimeOffset timestamp)
    {
        Id = id;
        Buyer = buyer;
        Status = EOrderStatus.Created;
        CreatedAt = timestamp;
        UpdatedAt = timestamp;
        _items.AddRange(items);
    }

    public static Order Create(Buyer buyer, IEnumerable<OrderItem> items, DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(buyer);
        ArgumentNullException.ThrowIfNull(items);

        var materialized = items.ToList();
        if (materialized.Count == 0)
        {
            throw new OrderMustHaveAtLeastOneItemException();
        }

        return new Order(Guid.NewGuid(), buyer, materialized, timestamp);
    }

    public void UpdateItems(IEnumerable<OrderItem> items, DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(items);
        EnsureMutable(nameof(UpdateItems));

        var materialized = items.ToList();
        if (materialized.Count == 0)
        {
            throw new OrderMustHaveAtLeastOneItemException();
        }

        _items.Clear();
        _items.AddRange(materialized);
        UpdatedAt = timestamp;
    }

    public void ChangeBuyer(Buyer buyer, DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(buyer);
        EnsureMutable(nameof(ChangeBuyer));

        Buyer = buyer;
        UpdatedAt = timestamp;
    }

    public void Process(DateTimeOffset timestamp)
    {
        if (Status != EOrderStatus.Created)
        {
            throw new InvalidOrderTransitionException(Status, EOrderStatus.Processed);
        }

        Status = EOrderStatus.Processed;
        UpdatedAt = timestamp;
    }

    public void Ship(DateTimeOffset timestamp)
    {
        if (Status != EOrderStatus.Processed)
        {
            throw new InvalidOrderTransitionException(Status, EOrderStatus.Shipped);
        }

        Status = EOrderStatus.Shipped;
        UpdatedAt = timestamp;
    }

    public void Cancel(DateTimeOffset timestamp)
    {
        if (Status is not (EOrderStatus.Created or EOrderStatus.Processed))
        {
            throw new InvalidOrderTransitionException(Status, EOrderStatus.Cancelled);
        }

        Status = EOrderStatus.Cancelled;
        UpdatedAt = timestamp;
    }

    private void EnsureMutable(string action)
    {
        if (Status != EOrderStatus.Created)
        {
            throw new InvalidOrderTransitionException(Status, action);
        }
    }
}
