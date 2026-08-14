namespace FleetOps.Order.Application.Abstractions;

public sealed record DriverReservationResult(
    bool Exists,
    bool IsReserved,
    string? Reason);