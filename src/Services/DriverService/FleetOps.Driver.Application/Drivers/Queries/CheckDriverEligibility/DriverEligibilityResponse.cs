using FleetOps.Driver.Domain.Drivers.Enums;

namespace FleetOps.Driver.Application.Drivers.Queries.CheckDriverEligibility
{
    public sealed record DriverEligibilityResponse(
        Guid DriverId,
        bool IsEligible,
        DriverStatus Status,
        string? Reason);
}