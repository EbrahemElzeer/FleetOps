using FleetOps.Order.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOps.Order.Infrastructure.Persistence.Configurations;

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.FromStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ToStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.ChangedAt)
            .IsRequired();

        builder.HasIndex(x => x.OrderId);
    }
}