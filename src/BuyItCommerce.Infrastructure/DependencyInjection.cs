using BuyItCommerce.Application.Abstractions.Caching;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Abstractions.Idempotency;
using BuyItCommerce.Application.Abstractions.Outbox;
using BuyItCommerce.Application.Configuration;
using BuyItCommerce.Infrastructure.Caching;
using BuyItCommerce.Infrastructure.Configuration;
using BuyItCommerce.Infrastructure.Outbox;
using BuyItCommerce.Infrastructure.Persistence;
using BuyItCommerce.Infrastructure.Persistence.Repositories;
using BuyItCommerce.Infrastructure.ReadModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace BuyItCommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddOptions<MongoOptions>()
            .Bind(configuration.GetSection(MongoOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddOptions<IdempotencyOptions>()
            .Bind(configuration.GetSection(IdempotencyOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddOptions<CacheOptions>()
            .Bind(configuration.GetSection(CacheOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddDbContext<OrdersDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("SqlServer"),
                sqlServer => sqlServer.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<OrdersDbContext>());
        services.AddScoped<IOrderWriteRepository, EfOrderWriteRepository>();
        services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();
        services.AddScoped<IOutboxWriter, EfOutboxWriter>();

        RegisterMongoSerializers();
        services.AddSingleton<IMongoClient>(_ => new MongoClient(configuration.GetConnectionString("MongoDb")));
        services.AddScoped<OrdersReadContext>();
        services.AddScoped<IOrderReadRepository, MongoOrderReadRepository>();

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        services.AddSingleton<OutboxSignal>();
        services.AddSingleton<IOutboxSignal>(provider => provider.GetRequiredService<OutboxSignal>());
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<MongoReadModelInitializer>();

        return services;
    }

    private static void RegisterMongoSerializers()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }
}
