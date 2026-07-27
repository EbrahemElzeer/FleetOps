using FleetOps.Driver.Domain.Common;
using FleetOps.Driver.Domain.Drivers.Enums;
using FleetOps.Driver.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Domain.Drivers
{
    public sealed class Driver: AggregateRoot
    {
        private Driver()
        {
            
        }


        private Driver(string fullName,string phoneNumber,VehicleType vehicleType,string vehiclePlateNumber): base(Guid.NewGuid())
        {
            FullName = fullName.Trim();
            PhoneNumber = phoneNumber.Trim();
            VehicleType = vehicleType;
            VehiclePlateNumber = vehiclePlateNumber.Trim().ToUpperInvariant();
         
            Status = DriverStatus.Offline;
            CreatedAt = DateTime.UtcNow;
        }

        public string FullName { get; private set; } = string.Empty;

        public string PhoneNumber { get; private set; } = string.Empty;

        public VehicleType VehicleType { get; private set; }

        public string VehiclePlateNumber { get; private set; } = string.Empty;

     

        public DriverStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? WentOnlineAt { get; private set; }

        public DateTime? WentOfflineAt { get; private set; }

        public DateTime? SuspendedAt { get; private set; }

        public DateTime? AssignedAt { get; private set; }

        public DateTime? TripStartedAt { get; private set; }

        public DateTime? ReturningStartedAt { get; private set; }
        public DriverSuspensionReason? SuspensionReason { get; private set; }

        public string? SuspensionNotes { get; private set; }
        public static Result<Driver> Create(string? fullName,string? phoneNumber,VehicleType vehicleType, string? vehiclePlateNumber)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return DriverErrors.FullNameRequired;

            if (string.IsNullOrWhiteSpace(phoneNumber))
                return DriverErrors.PhoneNumberRequired;

            if (string.IsNullOrWhiteSpace(vehiclePlateNumber))
                return DriverErrors.VehiclePlateNumberRequired;

        

            var driver = new Driver(
                fullName,
                phoneNumber,
                vehicleType,
                vehiclePlateNumber);
              

            return driver;
        }
        public Result GoOnline()
        {
            if (Status != DriverStatus.Offline)
                return DriverErrors.CannotGoOnline(Status);

            Status = DriverStatus.Available;
            WentOnlineAt = DateTime.UtcNow;

            return Result.Success;
        }

        public Result GoOffline()
        {
            if (Status != DriverStatus.Available)
                return DriverErrors.CannotGoOffline(Status);

            Status = DriverStatus.Offline;
            WentOfflineAt = DateTime.UtcNow;

            return Result.Success;
        }

        public Result Suspend(DriverSuspensionReason reason,string? notes)
        {
            if (Status == DriverStatus.OnTrip || Status == DriverStatus.Returning)
                return DriverErrors.CannotSuspend(Status);

            if (Status == DriverStatus.Suspended)
                return DriverErrors.CannotSuspend(Status);

            if(reason==DriverSuspensionReason.Other&&string.IsNullOrWhiteSpace(notes))
                return DriverErrors.SuspensionNotesRequired;

            Status = DriverStatus.Suspended;
            SuspendedAt = DateTime.UtcNow;
            SuspensionReason = reason;
            SuspensionNotes = string.IsNullOrWhiteSpace(notes)
          ? null
          : notes.Trim();

            return Result.Success;
        }

        public Result Activate()
        {
            if (Status != DriverStatus.Suspended)
                return DriverErrors.CannotActivate(Status);

            Status = DriverStatus.Offline;
            SuspensionReason = null;
            SuspensionNotes = null;

            return Result.Success;
        }

        public Result MarkAsAssigned()
        {
            if (Status != DriverStatus.Available)
                return DriverErrors.CannotMarkAsAssigned(Status);

            Status = DriverStatus.Assigned;
            AssignedAt = DateTime.UtcNow;

            return Result.Success;
        }

        public Result MarkAsOnTrip()
        {
            if (Status != DriverStatus.Assigned)
                return DriverErrors.CannotMarkAsOnTrip(Status);

            Status = DriverStatus.OnTrip;
            TripStartedAt = DateTime.UtcNow;

            return Result.Success;
        }

        public Result MarkAsReturning()
        {
            if (Status != DriverStatus.OnTrip)
                return DriverErrors.CannotMarkAsReturning(Status);

            Status = DriverStatus.Returning;
            ReturningStartedAt = DateTime.UtcNow;

            return Result.Success;
        }

        public Result MarkAsAvailable()
        {
            if (Status != DriverStatus.Assigned &&
                Status != DriverStatus.OnTrip &&
                Status != DriverStatus.Returning)
            {
                return DriverErrors.CannotMarkAsAvailable(Status);
            }

            Status = DriverStatus.Available;

            return Result.Success;
        }

        public Result CanBeAssigned()
        {
            if (Status != DriverStatus.Available)
                return DriverErrors.NotEligible(Status);

            return Result.Success;
        }
    }
}
