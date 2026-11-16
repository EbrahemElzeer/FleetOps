using FleetOps.Driver.Domain.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Infrastructure.Persistence.Configurations
{
    public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
           builder.ToTable("InboxMessages");
            builder.HasKey(x => x.MessageId);
            builder.Property(x => x.ProcessedAtUtc)
            .IsRequired();
        }
    }
}
