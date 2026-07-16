using FleetOps.Order.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOps.Order.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Domain.Orders.Order>
{
    public void Configure(EntityTypeBuilder<Domain.Orders.Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.CustomerPhone)
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(x => x.TrackingNumber, tracking =>
        {
            tracking.Property(x => x.Value)
                .HasColumnName("TrackingNumber")
                .HasMaxLength(30)
                .IsRequired();

            tracking.HasIndex(x => x.Value)
                .IsUnique();
        });

        builder.OwnsOne(x => x.PickupLocation, location =>
        {
            location.Property(x => x.Country)
                .HasColumnName("PickupCountry")
                .HasMaxLength(100)
                .IsRequired();

            location.Property(x => x.Governorate)
                .HasColumnName("PickupGovernorate")
                .HasMaxLength(100)
                .IsRequired();

            location.Property(x => x.City)
                .HasColumnName("PickupCity")
                .HasMaxLength(100)
                .IsRequired();

            location.Property(x => x.Area)
                .HasColumnName("PickupArea")
                .HasMaxLength(100)
                .IsRequired();

            location.Property(x => x.Street)
                .HasColumnName("PickupStreet")
                .HasMaxLength(200)
                .IsRequired();

            location.Property(x => x.BuildingNumber)
                .HasColumnName("PickupBuildingNumber")
                .HasMaxLength(50);

            location.Property(x => x.Landmark)
                .HasColumnName("PickupLandmark")
                .HasMaxLength(200);

            location.Property(x => x.Latitude)
                .HasColumnName("PickupLatitude")
                .HasColumnType("decimal(9,6)")
                .IsRequired();

            location.Property(x => x.Longitude)
                .HasColumnName("PickupLongitude")
                .HasColumnType("decimal(9,6)")
                .IsRequired();
        });

        builder.OwnsOne(x => x.DeliveryLocation, location =>
        {
            location.Property(x => x.Country)
                .HasColumnName("DeliveryCountry")
                .HasMaxLength(100)
                .IsRequired();

            location.Property(x => x.Governorate)
                .HasColumnName("DeliveryGovernorate")
                .HasMaxLength(100)
                .IsRequired();

            location.Property(x => x.City)
                .HasColumnName("DeliveryCity")
                .HasMaxLength(100)
                .IsRequired();

            location.Property(x => x.Area)
                .HasColumnName("DeliveryArea")
                .HasMaxLength(100)
                .IsRequired();

            location.Property(x => x.Street)
                .HasColumnName("DeliveryStreet")
                .HasMaxLength(200)
                .IsRequired();

            location.Property(x => x.BuildingNumber)
                .HasColumnName("DeliveryBuildingNumber")
                .HasMaxLength(50);

            location.Property(x => x.Landmark)
                .HasColumnName("DeliveryLandmark")
                .HasMaxLength(200);

            location.Property(x => x.Latitude)
                .HasColumnName("DeliveryLatitude")
                .HasColumnType("decimal(9,6)")
                .IsRequired();

            location.Property(x => x.Longitude)
                .HasColumnName("DeliveryLongitude")
                .HasColumnType("decimal(9,6)")
                .IsRequired();
        });

        builder.Property(x => x.DriverId);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();
        builder.HasIndex(x => x.DriverId);
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        builder.HasIndex(x => x.Status);
        builder.Property(x => x.AssignedAt);
        builder.Property(x => x.AcceptedAt);
        builder.Property(x => x.PickedUpAt);
        builder.Property(x => x.DeliveredAt);
        builder.Property(x => x.CancelledAt);

        builder.Property(x => x.DeliveryFailedAt);
        builder.Property(x => x.ReturnStartedAt);
        builder.Property(x => x.ReturnedAt);

        builder.Property(x => x.FailureReason)
            .HasConversion<int?>();

        builder.Property(x => x.DeliveryFailureNotes)
            .HasMaxLength(500);

        builder.HasMany(x => x.StatusHistories)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.StatusHistories)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}