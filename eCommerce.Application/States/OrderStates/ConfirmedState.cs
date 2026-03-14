using System;

namespace eCommerce.Application.States.OrderStates
{
    public class ConfirmedState : IOrderState
    {
        public string StatusName => "Đã xác nhận";
        public void Confirm(OrderContext context) => throw new InvalidOperationException("Đơn hàng đã được xác nhận.");
        public void Ship(OrderContext context) => context.TransitionTo(new ShippingState()); // Cho phép bàn giao
        public void Cancel(OrderContext context) => context.TransitionTo(new CanceledState());
        public void Complete(OrderContext context) => throw new InvalidOperationException("Đơn hàng chưa giao cho đơn vị vận chuyển.");
    }
}