using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.Orders
{
    public sealed record TrackingNumber
    {
        public string Value { get; private set; } = string.Empty;
        private TrackingNumber()
        {
        }
        private TrackingNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    "Tracking number cannot be empty.",
                    nameof(value));

            Value = value.Trim().ToUpperInvariant();
        }

        public static TrackingNumber Create()
        {
            var value = $"TRK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..21]
                .ToUpperInvariant();

            return new TrackingNumber(value);
        }
        public static TrackingNumber From(string value) =>  new TrackingNumber(value);
        

        public override string ToString() => Value;
    }
}
