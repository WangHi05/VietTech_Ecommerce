using System;

namespace eCommerce.Application.States.OrderStates
{
    public class CanceledState : IOrderState
    {
        public string StatusName => "Đã hủy";
        public void Confirm(OrderContext context) => throw new InvalidOperationException("Đơn hàng đã bị hủy, không thể xác nhận.");
        public void Ship(OrderContext context) => throw new InvalidOperationException("Đơn hàng đã bị hủy, không thể giao.");
        public void Complete(OrderContext context) => throw new InvalidOperationException("Đơn hàng đã bị hủy.");
        public void Cancel(OrderContext context) => throw new InvalidOperationException("Đơn hàng này đã được hủy trước đó.");
    }
}