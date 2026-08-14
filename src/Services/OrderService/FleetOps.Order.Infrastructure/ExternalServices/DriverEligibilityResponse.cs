using System.Text.Json.Serialization;

namespace FleetOps.Order.Infrastructure.ExternalServices;

internal sealed class DriverEligibilityResponse
{
    [JsonPropertyName("driverId")]
    public Guid DriverId { get; init; }

    [JsonPropertyName("isEligible")]
    public bool IsEligible { get; init; }

    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}