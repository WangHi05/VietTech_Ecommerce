namespace eCommerce.Application.DesignPatterns.ChainOfResponsibility;

using eCommerce.Core.Entities;

// PaymentCheckHandler: Handler thứ 3 (cuối cùng) trong chain
// Kiểm tra trạng thái thanh toán
// Nếu hợp lệ → hoàn tất xử lý đơn hàng
// Nếu không hợp lệ → dừng xử lý
public class PaymentCheckHandler : OrderHandler
{
    public override void Handle(Order order)
    {
        Console.WriteLine("\n--- [Handler 3] Kiểm tra trạng thái thanh toán ---");

        // Kiểm tra thanh toán
        if (order.PaymentStatus != "Đã thanh toán" && order.PaymentStatus != "Chưa thanh toán")
        {
            Console.WriteLine("[PaymentCheckHandler] ✗ Trạng thái thanh toán không hợp lệ, dừng xử lý.");
            return;
        }

        Console.WriteLine("[PaymentCheckHandler] ✓ Trạng thái thanh toán hợp lệ.");
        Console.WriteLine($"[PaymentCheckHandler] ✓ Hoàn tất xử lý đơn hàng #{order.Id}!\n");
    }
}
