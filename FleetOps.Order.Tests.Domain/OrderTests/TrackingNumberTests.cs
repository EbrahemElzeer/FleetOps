using FleetOps.Order.Domain.Orders;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FleetOps.Order.Tests.Domain.OrderTests
{
    public class TrackingNumberTests
    {
        [Fact]
        public void Create_ShouldGenerateValidTrackingNumber()
        {
            // Act
            var trackingNumber = TrackingNumber.Create();

            // Assert
            trackingNumber.Should().NotBeNull();
            trackingNumber.Value.Should().NotBeNullOrWhiteSpace();
            trackingNumber.Value.Should().StartWith("TRK-");
            trackingNumber.Value.Should().HaveLength(21);
            trackingNumber.ToString().Should().Be(trackingNumber.Value);
        }

        [Fact]
        public void Create_ShouldContainCurrentUtcDate()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow.ToString("yyyyMMdd");

            // Act
            var trackingNumber = TrackingNumber.Create();

            var afterCreation = DateTime.UtcNow.ToString("yyyyMMdd");

            // Assert
            trackingNumber.Value.Should().Match(value =>
                value.StartsWith($"TRK-{beforeCreation}-") ||
                value.StartsWith($"TRK-{afterCreation}-"));
        }

        [Fact]
        public void From_WithValidValue_ShouldTrimAndConvertToUppercase()
        {
            // Act
            var trackingNumber = TrackingNumber.From(
                "  trk-20260724-abc12345  ");

            // Assert
            trackingNumber.Value.Should().Be(
                "TRK-20260724-ABC12345");

            trackingNumber.ToString().Should().Be(
                "TRK-20260724-ABC12345");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void From_WithNullOrWhiteSpace_ShouldThrowArgumentException(string? value)
        {
            // Act
            Action action = () => TrackingNumber.From(value!);

            // Assert
            action.Should()
                .Throw<ArgumentException>()
                .WithMessage("*Tracking number cannot be empty*");
        }


        [Fact]
        public void Create_WhenCalledTwice_ShouldGenerateDifferentValues()
        {
            // Act
            var firstTrackingNumber = TrackingNumber.Create();
            var secondTrackingNumber = TrackingNumber.Create();

            // Assert
            firstTrackingNumber.Value
                .Should()
                .NotBe(secondTrackingNumber.Value);
        }
    }
}
