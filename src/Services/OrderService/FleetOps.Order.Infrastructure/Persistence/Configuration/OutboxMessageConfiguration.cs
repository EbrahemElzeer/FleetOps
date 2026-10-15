using FleetOps.Order.Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Infrastructure.Persistence.Configuration
{
    public sealed class OutboxMessageConfiguration
        : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.OccurredAt)
                .IsRequired();

            builder.Property(x => x.ProcessedAt);

            builder.Property(x => x.RetryCount)
                .IsRequired();

            builder.Property(x => x.Error)
                .HasMaxLength(2000);
        }
    }
}