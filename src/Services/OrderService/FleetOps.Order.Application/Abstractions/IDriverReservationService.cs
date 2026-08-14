namespace FleetOps.Order.Application.Abstractions;

public interface IDriverReservationService
{
    Task<DriverReservationResult> ReserveDriverAsync(
        Guid driverId,
        CancellationToken cancellationToken = default);
    Task ReleaseDriverAsync(
       Guid driverId,
       CancellationToken cancellationToken = default);
}