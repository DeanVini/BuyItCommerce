using BuyItCommerce.Domain.Exceptions;

namespace BuyItCommerce.Domain.Orders;

public sealed record Buyer
{
    public Guid Id { get; private set;}

    public string Name { get; private set; }

    private Buyer(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public static Buyer Create(Guid id, string name)
    {
        if (id == Guid.Empty || string.IsNullOrWhiteSpace(name))
        {
            throw new BuyerRequiredException();
        }

        return new Buyer(id, name.Trim());
    }
}
