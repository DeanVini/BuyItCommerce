using BuyItCommerce.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace BuyItCommerce.Infrastructure.Caching;

internal sealed class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(key, out T? cached) && cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken).ConfigureAwait(false);

        if (value is not null)
        {
            cache.Set(key, value, ttl);
        }

        return value;
    }

    public void Remove(string key) => cache.Remove(key);
}
