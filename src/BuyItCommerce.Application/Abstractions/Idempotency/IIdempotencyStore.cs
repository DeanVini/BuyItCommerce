namespace BuyItCommerce.Application.Abstractions.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> FindAsync(string key, CancellationToken cancellationToken);

    void Add(IdempotencyRecord record);
}
