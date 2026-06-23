using BuyItCommerce.Application.Abstractions.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuyItCommerce.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).ValueGeneratedNever();

        builder.Property(message => message.Type)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(message => message.Payload);
        builder.Property(message => message.OccurredAt);
        builder.Property(message => message.ProcessedAt);

        builder.HasIndex(message => message.ProcessedAt);
    }
}
