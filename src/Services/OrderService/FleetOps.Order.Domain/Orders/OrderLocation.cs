using FleetOps.Order.Domain.Common;

namespace FleetOps.Order.Domain.Orders;

public sealed record OrderLocation
{
    public string Country { get; private set; } = string.Empty;
    public string Governorate { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Area { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;
    public string? BuildingNumber { get; private set; }
    public string? Landmark { get; private set; }
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }

    private OrderLocation()
    {
    }

    private OrderLocation(
        string country,
        string governorate,
        string city,
        string area,
        string street,
        string? buildingNumber,
        string? landmark,
        decimal latitude,
        decimal longitude)
    {
        Country = country.Trim();
        Governorate = governorate.Trim();
        City = city.Trim();
        Area = area.Trim();
        Street = street.Trim();
        BuildingNumber = NormalizeOptional(buildingNumber);
        Landmark = NormalizeOptional(landmark);
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Result<OrderLocation> Create(
        string country,
        string governorate,
        string city,
        string area,
        string street,
        string? buildingNumber,
        string? landmark,
        decimal latitude,
        decimal longitude)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(country))
            errors.Add(OrderLocationErrors.CountryRequired);

        if (string.IsNullOrWhiteSpace(governorate))
            errors.Add(OrderLocationErrors.GovernorateRequired);

        if (string.IsNullOrWhiteSpace(city))
            errors.Add(OrderLocationErrors.CityRequired);

        if (string.IsNullOrWhiteSpace(area))
            errors.Add(OrderLocationErrors.AreaRequired);

        if (string.IsNullOrWhiteSpace(street))
            errors.Add(OrderLocationErrors.StreetRequired);

        if (latitude is < -90 or > 90)
            errors.Add(OrderLocationErrors.InvalidLatitude);

        if (longitude is < -180 or > 180)
            errors.Add(OrderLocationErrors.InvalidLongitude);

        if (errors.Count > 0)
            return Result<OrderLocation>.Failure(errors);

        return new OrderLocation(
            country,
            governorate,
            city,
            area,
            street,
            buildingNumber,
            landmark,
            latitude,
            longitude);
    }

    public string GetFormattedAddress()
    {
        var parts = new List<string>
        {
            Street
        };

        if (!string.IsNullOrWhiteSpace(BuildingNumber))
            parts.Add($"Building {BuildingNumber}");

        parts.Add(Area);
        parts.Add(City);
        parts.Add(Governorate);
        parts.Add(Country);

        if (!string.IsNullOrWhiteSpace(Landmark))
            parts.Add($"Near {Landmark}");

        return string.Join(", ", parts);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}