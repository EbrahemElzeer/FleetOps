using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Common;
using FleetOps.Order.Domain.Orders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Commands.AssignDriver
{
    public class  AssignDriverCommandHandler : IRequestHandler<AssignDriverCommand,Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AssignDriverCommandHandler(IOrderRepository orderRepository,IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result>  Handle(AssignDriverCommand request, CancellationToken cancellationToken)
        {

            if (request.DriverId == Guid.Empty)
            {
                return Error.Validation(
                    code: "Orders.InvalidDriverId",
                    description: "Driver id is required.");
            }

            var order= await _orderRepository.GetByIdAsync(request.OrderId,cancellationToken);
            if (order == null) {
                return Error.NotFound(code: "Order.NotFound",
                        description: $"The order with ID '{request.OrderId}' was not found."); }
          
            if (order.Status != OrderStatus.Pending)
                return Error.Conflict(code: "Order.Conflict", description: $"The Status Order must be Pending");
            order.AssignDriver(driverId: request.DriverId);

            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;

        }
    }
}
