namespace FleetOps.Order.Infrastructure.ExternalServices;

public sealed class DriverServiceOptions
{
    public const string SectionName = "DriverService";

    public string BaseUrl { get; init; } = string.Empty;
}