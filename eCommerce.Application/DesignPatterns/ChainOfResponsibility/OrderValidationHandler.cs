namespace eCommerce.Application.DesignPatterns.ChainOfResponsibility;

using eCommerce.Core.Entities;

// OrderValidationHandler: Handler đầu tiên trong chain
// Kiểm tra dữ liệu đơn hàng hợp lệ (địa chỉ, tổng tiền, v.v.)
// Nếu hợp lệ → chuyển sang handler tiếp theo
// Nếu không hợp lệ → dừng xử lý
public class OrderValidationHandler : OrderHandler
{
    public override void Handle(Order order)
    {
        Console.WriteLine("\n--- [Handler 1] Kiểm tra dữ liệu đơn hàng ---");

        // Kiểm tra dữ liệu
        if (order == null || string.IsNullOrEmpty(order.ShippingAddress) || order.Total <= 0)
        {
            Console.WriteLine("[OrderValidationHandler] ✗ Dữ liệu không hợp lệ, dừng xử lý.");
            return;
        }

        Console.WriteLine("[OrderValidationHandler] ✓ Dữ liệu hợp lệ, chuyển sang handler tiếp theo...");
        PassToNext(order);
    }
}
