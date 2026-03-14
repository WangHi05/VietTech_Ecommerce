using eCommerce.Core.Entities;

namespace eCommerce.Application.States.OrderStates
{
    public interface IOrderState
    {
        // Tên trạng thái để lưu vào Database ("Chờ xác nhận", "Đang giao hàng")
        string StatusName { get; }

        // Các hành động có thể làm với đơn hàng
        void Confirm(OrderContext context); // Xác nhận
        void Ship(OrderContext context);     // Giao hàng
        void Complete(OrderContext context); // Hoàn thành
        void Cancel(OrderContext context);   // Hủy đơn
    }
}