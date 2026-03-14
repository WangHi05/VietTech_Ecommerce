using eCommerce.Core.Entities;

namespace eCommerce.Application.States.OrderStates
{
    public class OrderContext
    {
        public Order Order { get; }
        private IOrderState _currentState;

        public OrderContext(Order order)
        {
            Order = order;
            // Dựa vào chuỗi trạng thái trong DB để khởi tạo đúng class State
            _currentState = GetStateFromName(order.Status);
        }

        // Hàm này để các Class State gọi khi nó muốn chuyển sang State khác
        public void TransitionTo(IOrderState state)
        {
            _currentState = state;
            Order.Status = state.StatusName; // Cập nhật lại chuỗi Status cho Entity Order
        }

        // Ủy quyền (Delegate) các hành động cho State hiện tại xử lý
        public void Ship() => _currentState.Ship(this);
        public void Complete() => _currentState.Complete(this);
        public void Cancel() => _currentState.Cancel(this);

        // Factory Pattern thu nhỏ: Sinh ra State object tương ứng
        private IOrderState GetStateFromName(string? status)
        {
            return status switch
            {
                "Đang giao hàng" => new ShippingState(),
                "Hoàn thành" => new CompletedState(),
                "Đã hủy" => new CanceledState(),
                _ => new PendingState() // Mặc định nếu null hoặc "Chờ xác nhận"
            };
        }
        public string GetCurrentStatus() => _currentState.StatusName;
    }
}