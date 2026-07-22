using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Domain.Common;
using FleetOps.Order.Domain.Orders;
using FleetOps.Order.Domain.Orders.Enums;
using MediatR;

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

            var order= await _orderRepository.GetByIdAsync(request.OrderId,cancellationToken);
            if (order is null) return OrderErrors.NotFound(request.OrderId);

            var result=  order.AssignDriver(driverId: request.DriverId);
            if (result.IsFailure) return result;

            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
