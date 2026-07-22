using FleetOps.Order.Domain.Common;

namespace FleetOps.Order.Domain.Orders;

public static class OrderLocationErrors
{
    public static readonly Error CountryRequired = Error.Validation(
        "OrderLocation.CountryRequired",
        "Country is required.");

    public static readonly Error GovernorateRequired = Error.Validation(
        "OrderLocation.GovernorateRequired",
        "Governorate is required.");

    public static readonly Error CityRequired = Error.Validation(
        "OrderLocation.CityRequired",
        "City is required.");

    public static readonly Error AreaRequired = Error.Validation(
        "OrderLocation.AreaRequired",
        "Area is required.");

    public static readonly Error StreetRequired = Error.Validation(
        "OrderLocation.StreetRequired",
        "Street is required.");

    public static readonly Error InvalidLatitude = Error.Validation(
        "OrderLocation.InvalidLatitude",
        "Latitude must be between -90 and 90.");

    public static readonly Error InvalidLongitude = Error.Validation(
        "OrderLocation.InvalidLongitude",
        "Longitude must be between -180 and 180.");
}