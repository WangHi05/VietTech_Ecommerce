namespace eCommerce.Application.DesignPatterns.ChainOfResponsibility;

using eCommerce.Core.Entities;

// StockCheckHandler: Handler thứ 2 trong chain
// Kiểm tra tồn kho (hàng đủ hay không)
// Nếu đủ → chuyển sang handler tiếp theo
// Nếu không đủ → dừng xử lý
public class StockCheckHandler : OrderHandler
{
    public override void Handle(Order order)
    {
        Console.WriteLine("\n--- [Handler 2] Kiểm tra tồn kho ---");

        // Giả lập: nếu order.Total > 0 thì có hàng
        if (order.Total <= 0)
        {
            Console.WriteLine("[StockCheckHandler] ✗ Tồn kho không đủ, dừng xử lý.");
            return;
        }

        Console.WriteLine("[StockCheckHandler] ✓ Tồn kho đủ, chuyển sang handler tiếp theo...");
        PassToNext(order);
    }
}
