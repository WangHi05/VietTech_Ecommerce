namespace eCommerce.Application.DesignPatterns.Command;

using eCommerce.Core.Entities;
using eCommerce.Application.Services;

// CreateOrderCommand: Implement ICommand
// Đóng gói yêu cầu tạo đơn hàng vào một object command
// Áp dụng: Xử lý yêu cầu tạo đơn, dễ dàng queue, retry, hoặc audit log
public class CreateOrderCommand : ICommand
{
    private readonly IOrderService _orderService;
    private readonly Order _order;

    public CreateOrderCommand(IOrderService orderService, Order order)
    {
        _orderService = orderService;
        _order = order;
    }

    public void Execute()
    {
        // Ghi log
        Console.WriteLine($"[CreateOrderCommand] Đang tạo đơn hàng #{_order.Id}: {_order.ShippingAddress} - Tổng: {_order.Total}đ");
        
        // Gọi service thật - sử dụng Task.Run để chạy async trong sync context
        Task.Run(async () => await _orderService.PlaceOrderAsync(_order)).Wait();
        
        Console.WriteLine($"[CreateOrderCommand] ✓ Đơn hàng #{_order.Id} đã được tạo thành công");
    }
}
