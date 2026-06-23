using BuyItCommerce.Application.Abstractions.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuyItCommerce.Infrastructure.Persistence.Configurations;

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyKeys");

        builder.HasKey(record => record.Key);
        builder.Property(record => record.Key).HasMaxLength(128);
        builder.Property(record => record.RequestHash).HasMaxLength(64);
        builder.Property(record => record.ResponsePayload);
        builder.Property(record => record.CreatedAt);
        builder.Property(record => record.ExpiresAt);
    }
}
