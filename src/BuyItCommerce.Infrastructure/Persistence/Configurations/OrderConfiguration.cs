using BuyItCommerce.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuyItCommerce.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(order => order.CreatedAt);
        builder.Property(order => order.UpdatedAt);

        builder.Ignore(order => order.TotalAmount);

        builder.OwnsOne(order => order.Buyer, buyer =>
        {
            buyer.Property(value => value.Id).HasColumnName("BuyerId");
            buyer.Property(value => value.Name).HasColumnName("BuyerName").HasMaxLength(200);
            buyer.HasIndex(value => value.Id);
        });
        builder.Navigation(order => order.Buyer).IsRequired();

        builder.OwnsMany(order => order.Items, items =>
        {
            items.ToTable("OrderItems");
            items.WithOwner().HasForeignKey("OrderId");
            items.HasKey("OrderId", "ProductId");
            items.Property(item => item.ProductId);
            items.Property(item => item.ProductName).HasMaxLength(200);
            items.Property(item => item.UnitPrice).HasPrecision(18, 2);
            items.Property(item => item.Quantity);
            items.Ignore(item => item.LineTotal);
        });

        builder.Navigation(order => order.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_items");

        builder.HasIndex(order => order.Status);
    }
}
