using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Domain.Common;
using FleetOps.Order.Domain.Orders;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.AcceptOrder
{
    public sealed class AcceptOrderCommandHandler : IRequestHandler<AcceptOrderCommand, Result>
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;

        public AcceptOrderCommandHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
        }
        public async Task<Result> Handle(AcceptOrderCommand request, CancellationToken cancellationToken)
        {


            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null) return OrderErrors.NotFound(request.OrderId);

            var result = order.AcceptByDriver(request.DriverId);
            if (result.IsFailure) return result;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
}
