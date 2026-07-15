using FleetOps.Order.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.Orders
{
    public sealed class OrderStatusHistory:Entity
    {
        public Guid OrderId { get; private set; }

        public OrderStatus FromStatus { get; private set; }

        public OrderStatus ToStatus { get; private set; }

        public string? Notes { get; private set; }

        public DateTime ChangedAt { get; private set; }

        private OrderStatusHistory()
        {
        }

        internal OrderStatusHistory(
            Guid orderId,
            OrderStatus fromStatus,
            OrderStatus toStatus,
            string? notes) : base(Guid.NewGuid())
        {

            if (orderId == Guid.Empty)
                throw new ArgumentException(
                    "Order ID cannot be empty.",
                    nameof(orderId));

            if (fromStatus == toStatus)
                throw new ArgumentException(
                    "From status and to status cannot be the same.");


            OrderId = orderId;
            FromStatus = fromStatus;
            ToStatus = toStatus;
            Notes = notes;
            ChangedAt = DateTime.UtcNow;
        }

    }
}
