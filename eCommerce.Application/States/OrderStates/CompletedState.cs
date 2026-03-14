using System;

namespace eCommerce.Application.States.OrderStates
{
    public class CompletedState : IOrderState
    {
        public string StatusName => "Hoàn thành";
        public void Confirm(OrderContext context) => throw new InvalidOperationException("Đơn hàng đã hoàn thành, không thể xác nhận.");
        public void Ship(OrderContext context) => throw new InvalidOperationException("Đơn hàng đã hoàn thành.");
        public void Complete(OrderContext context) => throw new InvalidOperationException("Đơn hàng đã hoàn thành rồi.");
        public void Cancel(OrderContext context) => throw new InvalidOperationException("Không thể hủy đơn hàng đã hoàn thành.");
    }
}