using FleetOps.Order.Domain.Orders;
using FluentAssertions;
using Xunit;
namespace FleetOps.Order.Tests.Domain.OrderTests
{
    public class OrderLocationTests
    {
        [Fact]
        public void Create_WithValidData_ShouldReturnSuccessfulResult()
        {
            // Arrange
            const string country = "Egypt";
            const string governorate = "Cairo";
            const string city = "Nasr City";
            const string area = "Abbas El Akkad";
            const string street = "Mostafa El Nahas";
            const string buildingNumber = "25";
            const string landmark = "City Stars";
            const decimal latitude = 30.0566m;
            const decimal longitude = 31.3301m;

            // Act
            var result = OrderLocation.Create(
                country,
                governorate,
                city,
                area,
                street,
                buildingNumber,
                landmark,
                latitude,
                longitude);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            result.Value.Country.Should().Be(country);
            result.Value.Governorate.Should().Be(governorate);
            result.Value.City.Should().Be(city);
            result.Value.Area.Should().Be(area);
            result.Value.Street.Should().Be(street);
            result.Value.BuildingNumber.Should().Be(buildingNumber);
            result.Value.Landmark.Should().Be(landmark);
            result.Value.Latitude.Should().Be(latitude);
            result.Value.Longitude.Should().Be(longitude);
        }


        [Fact]
        public void Create_WhenCountryIsEmpty_ShouldReturnCountryRequiredError()
        {
            // Act
            var result = OrderLocation.Create(
                country: "",
                governorate: "Cairo",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: "25",
                landmark: "City Stars",
                latitude: 30.0566m,
                longitude: 31.3301m);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderLocationErrors.CountryRequired);
        }
        [Fact]
        public void Create_WhenGovernorateIsEmpty_ShouldReturnGovernorateRequiredError()
        {
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: "25",
                landmark: "City Stars",
                latitude: 30.0566m,
                longitude: 31.3301m);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderLocationErrors.GovernorateRequired);
        }

        [Fact]
        public void Create_WhenCityIsEmpty_ShouldReturnCityRequiredError()
        {
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: "25",
                landmark: "City Stars",
                latitude: 30.0566m,
                longitude: 31.3301m);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderLocationErrors.CityRequired);
        }

        [Fact]
        public void Create_WhenAreaIsEmpty_ShouldReturnAreaRequiredError()
        {
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "Nasr City",
                area: "",
                street: "Mostafa El Nahas",
                buildingNumber: "25",
                landmark: "City Stars",
                latitude: 30.0566m,
                longitude: 31.3301m);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderLocationErrors.AreaRequired);
        }

        [Fact]
        public void Create_WhenStreetIsEmpty_ShouldReturnStreetRequiredError()
        {
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "",
                buildingNumber: "25",
                landmark: "City Stars",
                latitude: 30.0566m,
                longitude: 31.3301m);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderLocationErrors.StreetRequired);
        }

        [Theory]
        [InlineData(-90.1)]
        [InlineData(90.1)]
        public void Create_WhenLatitudeIsOutOfRange_ShouldReturnInvalidLatitudeError(decimal latitude)
        {
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: "25",
                landmark: "City Stars",
                latitude: latitude,
                longitude: 31.3301m);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderLocationErrors.InvalidLatitude);
        }

        [Theory]
        [InlineData(-180.1)]
        [InlineData(180.1)]
        public void Create_WhenLongitudeIsOutOfRange_ShouldReturnInvalidLongitudeError(decimal longitude)
        {
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: "25",
                landmark: "City Stars",
                latitude: 30.0566m,
                longitude: longitude);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderLocationErrors.InvalidLongitude);
        }

        [Theory]
        [InlineData(-90)]
        [InlineData(90)]
        public void Create_WhenLatitudeIsOnBoundary_ShouldReturnSuccess(decimal latitude)
        {
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: "25",
                landmark: "City Stars",
                latitude: latitude,
                longitude: 31.3301m);

            result.IsSuccess.Should().BeTrue();
            result.Value.Latitude.Should().Be(latitude);
        }

        [Theory]
        [InlineData(-180)]
        [InlineData(180)]
        public void Create_WhenLongitudeIsOnBoundary_ShouldReturnSuccess(decimal longitude)
        {
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: "25",
                landmark: "City Stars",
                latitude: 30.0566m,
                longitude: longitude);

            result.IsSuccess.Should().BeTrue();
            result.Value.Longitude.Should().Be(longitude);
        }

        [Fact]
        public void Create_WhenTextValuesContainSpaces_ShouldTrimValues()
        {
            var result = OrderLocation.Create(
                country: "  Egypt  ",
                governorate: "  Cairo  ",
                city: "  Nasr City  ",
                area: "  Abbas El Akkad  ",
                street: "  Mostafa El Nahas  ",
                buildingNumber: "  25  ",
                landmark: "  City Stars  ",
                latitude: 30.0566m,
                longitude: 31.3301m);

            result.IsSuccess.Should().BeTrue();

            result.Value.Country.Should().Be("Egypt");
            result.Value.Governorate.Should().Be("Cairo");
            result.Value.City.Should().Be("Nasr City");
            result.Value.Area.Should().Be("Abbas El Akkad");
            result.Value.Street.Should().Be("Mostafa El Nahas");
            result.Value.BuildingNumber.Should().Be("25");
            result.Value.Landmark.Should().Be("City Stars");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WhenBuildingNumberIsNullOrWhiteSpace_ShouldSetItToNull(string? buildingNumber)
        {
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: buildingNumber,
                landmark: "City Stars",
                latitude: 30.0566m,
                longitude: 31.3301m);

            result.IsSuccess.Should().BeTrue();
            result.Value.BuildingNumber.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WhenLandmarkIsNullOrWhiteSpace_ShouldSetItToNull(string? landmark)
        {
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: "25",
                landmark: landmark,
                latitude: 30.0566m,
                longitude: 31.3301m);

            result.IsSuccess.Should().BeTrue();
            result.Value.Landmark.Should().BeNull();
        }

        [Fact]
        public void GetFormattedAddress_WhenAllValuesExist_ShouldReturnFormattedAddress()
        {
            // Arrange
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: "25",
                landmark: "City Stars",
                latitude: 30.0566m,
                longitude: 31.3301m);

            // Act
            var formattedAddress = result.Value.GetFormattedAddress();

            // Assert
            formattedAddress.Should().Be(
                "Mostafa El Nahas, Building 25, Abbas El Akkad, Nasr City, Cairo, Egypt, Near City Stars");
        }

        [Fact]
        public void GetFormattedAddress_WhenOptionalValuesAreMissing_ShouldExcludeThem()
        {
            // Arrange
            var result = OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: null,
                landmark: null,
                latitude: 30.0566m,
                longitude: 31.3301m);

            // Act
            var formattedAddress = result.Value.GetFormattedAddress();

            // Assert
            formattedAddress.Should().Be(
                "Mostafa El Nahas, Abbas El Akkad, Nasr City, Cairo, Egypt");
        }



        [Fact]
        public void Create_WhenMultipleValuesAreInvalid_ShouldReturnAllErrors()
        {
            // Act
            var result = OrderLocation.Create(
                country: "",
                governorate: "",
                city: "",
                area: "",
                street: "",
                buildingNumber: null,
                landmark: null,
                latitude: 100m,
                longitude: 200m);

            // Assert
            result.IsFailure.Should().BeTrue();

            result.Errors.Should().Contain(OrderLocationErrors.CountryRequired);
            result.Errors.Should().Contain(OrderLocationErrors.GovernorateRequired);
            result.Errors.Should().Contain(OrderLocationErrors.CityRequired);
            result.Errors.Should().Contain(OrderLocationErrors.AreaRequired);
            result.Errors.Should().Contain(OrderLocationErrors.StreetRequired);
            result.Errors.Should().Contain(OrderLocationErrors.InvalidLatitude);
            result.Errors.Should().Contain(OrderLocationErrors.InvalidLongitude);

            result.Errors.Should().HaveCount(7);
        }


    }
}