using System;

namespace eCommerce.Application.States.OrderStates
{
    public class PendingState : IOrderState
    {
        public string StatusName => "Chờ xác nhận";

        public void Confirm(OrderContext context)
        {
            context.TransitionTo(new ConfirmedState());
        } 
        
        public void Ship(OrderContext context) 
        {
            throw new InvalidOperationException("Phải xác nhận đơn hàng trước khi giao.");
        }

        public void Complete(OrderContext context)
        {
            // Chờ xác nhận -> Hoàn thành (Không hợp lệ, phải giao hàng trước)
            throw new InvalidOperationException("Không thể hoàn thành đơn hàng chưa được giao.");
        }

        public void Cancel(OrderContext context)
        {
            // Chờ xác nhận -> Hủy (Hợp lệ)
            context.TransitionTo(new CanceledState());
        }
    }
}