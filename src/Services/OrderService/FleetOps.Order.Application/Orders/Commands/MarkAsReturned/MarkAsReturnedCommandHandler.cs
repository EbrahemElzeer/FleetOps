using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Common;
using FleetOps.Order.Application.Orders.Commands.MarkDeliveryFailed;
using FleetOps.Order.Domain.Common;
using FleetOps.Order.Domain.Orders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Commands.MarkAsReturned
{
    public sealed class MarkAsReturnedCommandHandler : IRequestHandler<MarkAsReturnedCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        public MarkAsReturnedCommandHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
        }

        public async Task<Result> Handle(MarkAsReturnedCommand request, CancellationToken cancellationToken)
        {
            if (request.OrderId == Guid.Empty) return OrderErrors.OrderIdRequired;

            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null) return OrderErrors.NotFound(request.OrderId);

            var result= order.MarkAsReturned(request.DriverId);
            if (result.IsFailure) return result;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }

}
