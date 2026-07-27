using FleetOps.Driver.Domain.Drivers.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DriverAggregate = FleetOps.Driver.Domain.Drivers.Driver;

namespace FleetOps.Driver.Infrastructure.Persistence.Configurations
{
    public sealed class DriverConfiguration : IEntityTypeConfiguration<DriverAggregate>
       
    {
        public void Configure(EntityTypeBuilder<DriverAggregate> builder)
        {
            builder.ToTable("Drivers");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.FullName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(d => d.PhoneNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(d => d.VehiclePlateNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(d => d.VehicleType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(d => d.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.Property(d => d.WentOnlineAt);

            builder.Property(d => d.WentOfflineAt);

            builder.Property(d => d.SuspendedAt);

            builder.Property(d => d.AssignedAt);

            builder.Property(d => d.TripStartedAt);

            builder.Property(d => d.ReturningStartedAt);

            builder.Property(d => d.SuspensionReason)
                .HasConversion<int?>();

            builder.Property(d => d.SuspensionNotes)
                .HasMaxLength(500);

            builder.HasIndex(d => d.PhoneNumber)
                .IsUnique();

            builder.HasIndex(d => d.VehiclePlateNumber)
                .IsUnique();
        }
    }
}