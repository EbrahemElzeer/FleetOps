using FleetOps.Order.Domain.Sagas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Infrastructure.Persistence.Configuration
{
    public sealed class AssignDriverSagaConfiguration : IEntityTypeConfiguration<AssignDriverSagaState>
    {
        public void Configure(EntityTypeBuilder<AssignDriverSagaState> builder)
        {
            builder.ToTable("AssignDriverSagaStates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.LastError)
                .HasMaxLength(1000);

            builder.HasIndex(x => x.OrderId);

            builder.HasIndex(x => x.Status);
        }
    }
}
