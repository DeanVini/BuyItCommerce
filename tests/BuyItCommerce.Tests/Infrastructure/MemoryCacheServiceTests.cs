using BuyItCommerce.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;

namespace BuyItCommerce.Tests.Infrastructure;

public class MemoryCacheServiceTests
{
    private static MemoryCacheService CreateService() =>
        new(new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public async Task GetOrCreateAsync_CachesValue_FactoryRunsOnce()
    {
        var service = CreateService();
        var calls = 0;

        Task<string> Factory(CancellationToken _)
        {
            calls++;
            return Task.FromResult("value");
        }

        var first = await service.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1), CancellationToken.None);
        var second = await service.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1), CancellationToken.None);

        first.Should().Be("value");
        second.Should().Be("value");
        calls.Should().Be(1);
    }

    [Fact]
    public async Task Remove_EvictsEntry_FactoryRunsAgain()
    {
        var service = CreateService();
        var calls = 0;

        Task<string> Factory(CancellationToken _)
        {
            calls++;
            return Task.FromResult("value");
        }

        await service.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1), CancellationToken.None);
        service.Remove("key");
        await service.GetOrCreateAsync("key", Factory, TimeSpan.FromMinutes(1), CancellationToken.None);

        calls.Should().Be(2);
    }
}
