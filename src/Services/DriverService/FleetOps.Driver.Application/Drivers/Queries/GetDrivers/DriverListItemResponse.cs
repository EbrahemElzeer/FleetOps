using FleetOps.Driver.Domain.Drivers.Enums;

namespace FleetOps.Driver.Application.Drivers.Queries.GetDrivers
{
    public sealed record DriverListItemResponse(
        Guid Id,
        string FullName,
        string PhoneNumber,
        VehicleType VehicleType,
        string VehiclePlateNumber,
        DriverStatus Status,
        DateTime CreatedAt);
}