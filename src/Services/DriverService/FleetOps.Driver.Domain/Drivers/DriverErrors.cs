using FleetOps.Driver.Domain.Common;
using FleetOps.Driver.Domain.Drivers.Enums;

namespace FleetOps.Driver.Domain.Drivers;

public static class DriverErrors
{
    public static readonly Error FullNameRequired = Error.Validation(
        code: "Drivers.FullNameRequired",
        description: "Driver full name is required.");

    public static readonly Error PhoneNumberRequired = Error.Validation(
        code: "Drivers.PhoneNumberRequired",
        description: "Driver phone number is required.");

    public static readonly Error VehiclePlateNumberRequired = Error.Validation(
        code: "Drivers.VehiclePlateNumberRequired",
        description: "Vehicle plate number is required.");

  

    public static Error CannotGoOnline(DriverStatus status) => Error.Conflict(
        code: "Drivers.CannotGoOnline",
        description: $"Driver cannot go online while status is '{status}'.");

    public static Error CannotGoOffline(DriverStatus status) => Error.Conflict(
        code: "Drivers.CannotGoOffline",
        description: $"Driver cannot go offline while status is '{status}'.");

    public static Error CannotSuspend(DriverStatus status) => Error.Conflict(
        code: "Drivers.CannotSuspend",
        description: $"Driver cannot be suspended while status is '{status}'.");

    public static Error CannotActivate(DriverStatus status) => Error.Conflict(
        code: "Drivers.CannotActivate",
        description: $"Driver cannot be activated while status is '{status}'.");

    public static Error CannotMarkAsAssigned(DriverStatus status) => Error.Conflict(
        code: "Drivers.CannotMarkAsAssigned",
        description: $"Driver cannot be marked as assigned while status is '{status}'.");

    public static Error CannotMarkAsOnTrip(DriverStatus status) => Error.Conflict(
        code: "Drivers.CannotMarkAsOnTrip",
        description: $"Driver cannot be marked as on trip while status is '{status}'.");

    public static Error CannotMarkAsReturning(DriverStatus status) => Error.Conflict(
        code: "Drivers.CannotMarkAsReturning",
        description: $"Driver cannot be marked as returning while status is '{status}'.");

    public static Error CannotMarkAsAvailable(DriverStatus status) => Error.Conflict(
        code: "Drivers.CannotMarkAsAvailable",
        description: $"Driver cannot be marked as available while status is '{status}'.");

    public static readonly Error SuspensionNotesRequired = Error.Validation(
      code: "Drivers.SuspensionNotesRequired",
      description: "Suspension notes are required when the reason is Other.");

    public static Error NotEligible(DriverStatus status) => Error.Conflict(
        code: "Drivers.NotEligible",
        description: $"Driver is not eligible for assignment while status is '{status}'.");
}