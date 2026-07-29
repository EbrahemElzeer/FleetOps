using FleetOps.Driver.Domain.Drivers.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Application.Drivers.Queries.GetDriverById
{
    public sealed record DriverDetailsResponse(
         Guid Id,
         string FullName,
         string PhoneNumber,
         VehicleType VehicleType,
         string VehiclePlateNumber,
         DriverStatus Status,
         DateTime CreatedAt,
         DateTime? WentOnlineAt,
         DateTime? WentOfflineAt,
         DateTime? SuspendedAt,
         DateTime? AssignedAt,
         DateTime? TripStartedAt,
         DateTime? ReturningStartedAt,
         DriverSuspensionReason? SuspensionReason,
         string? SuspensionNotes
     );
}
