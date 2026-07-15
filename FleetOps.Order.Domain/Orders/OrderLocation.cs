using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.Orders
{
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
            ValidateRequired(country, nameof(country), "Country is required.");

            ValidateRequired(
                governorate,
                nameof(governorate),
                "Governorate is required.");

            ValidateRequired(city, nameof(city), "City is required.");

            ValidateRequired(area, nameof(area), "Area is required.");

            ValidateRequired(street, nameof(street), "Street is required.");

            ValidateCoordinates(latitude, longitude);

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

        public static OrderLocation Create(
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











        private static void ValidateRequired(
       string value,
       string parameterName,
       string message)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(message, parameterName);
        }

        private static void ValidateCoordinates(
            decimal latitude,
            decimal longitude)
        {
            if (latitude is < -90 or > 90)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(latitude),
                    "Latitude must be between -90 and 90.");
            }

            if (longitude is < -180 or > 180)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(longitude),
                    "Longitude must be between -180 and 180.");
            }
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
