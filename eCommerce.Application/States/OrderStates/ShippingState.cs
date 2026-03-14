using System;

namespace eCommerce.Application.States.OrderStates
{
    public class ShippingState : IOrderState
    {
        public string StatusName => "Đang giao hàng";

        public void Confirm(OrderContext context) 
        {
            throw new InvalidOperationException("Đơn hàng đang được giao, không thể xác nhận.");
        }
        public void Ship(OrderContext context)
        {
            throw new InvalidOperationException("Đơn hàng này ĐANG được giao rồi.");
        }

        public void Complete(OrderContext context)
        {
            // Đang giao -> Hoàn thành (Hợp lệ)
            context.TransitionTo(new CompletedState());
        }

        public void Cancel(OrderContext context)
        {
            throw new InvalidOperationException("Đơn hàng đang trên đường giao, không thể hủy. Vui lòng từ chối nhận hàng.");
        }
    }
}