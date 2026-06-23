using BuyItCommerce.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BuyItCommerce.Infrastructure.Persistence;

internal sealed class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer")
            ?? "Server=localhost;Database=BuyItCommerce;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new OrdersDbContext(options, new OutboxSignal());
    }
}
