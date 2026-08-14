using System.Net;
using FleetOps.Order.Application.Abstractions;

namespace FleetOps.Order.Infrastructure.ExternalServices;

public sealed class DriverReservationService : IDriverReservationService
{
    private readonly HttpClient _httpClient;

    public DriverReservationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DriverReservationResult> ReserveDriverAsync(
        Guid driverId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsync(
            $"/api/drivers/{driverId}/reserve",
            content: null,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new DriverReservationResult(
                Exists: false,
                IsReserved: false,
                Reason: "Driver was not found.");
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return new DriverReservationResult(
                Exists: true,
                IsReserved: false,
                Reason: "Driver is not available for assignment.");
        }

        response.EnsureSuccessStatusCode();

        return new DriverReservationResult(
            Exists: true,
            IsReserved: true,
            Reason: null);
    }


    public async Task ReleaseDriverAsync(
      Guid driverId,
      CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsync(
            $"/api/drivers/{driverId}/release",
            content: null,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return;

        if (response.StatusCode == HttpStatusCode.Conflict)
            return;

        response.EnsureSuccessStatusCode();
    }
}