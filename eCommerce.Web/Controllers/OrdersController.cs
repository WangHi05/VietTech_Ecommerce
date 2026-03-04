using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Web.Controllers
{
    /// <summary>
    /// TEMPLATE METHOD PATTERN — Class Con
    /// Kế thừa OrderProcessorTemplateMethod, chỉ implement bước thanh toán riêng.
    /// Các bước còn lại (validate, check stock, lưu DB) dùng của base class.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : OrderProcessorTemplateMethod
    {
        private readonly IConfiguration _configuration;

        public OrdersController(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IConfiguration configuration)
            : base(orderRepository, productRepository)
        {
            _configuration = configuration;
        }

        // POST: api/Orders  — Gọi Template Method từ base class
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] Order order)
        {
            // Gọi Template Method → bộ khung tự chạy hết các bước
            return await PlaceOrderAsync(order);
        }

        // ============================================================
        // Bước KHÁC NHAU — implement riêng, tùy phương thức thanh toán
        // ============================================================
        protected override Task<string> ProcessPaymentAsync(Order order)
        {
            if (order.PaymentMethod == "VnPay")
            {
                // VnPay: chờ thanh toán online
                order.PaymentStatus = "Chờ thanh toán";
                order.Status = "Đang chờ";
                Console.WriteLine($"[VnPay] Tạo link thanh toán cho đơn hàng của {order.ShippingName}");
            }
            else
            {
                // COD: thanh toán khi nhận hàng
                order.PaymentStatus = "Chưa thanh toán";
                order.Status = "Đang chờ";
                Console.WriteLine($"[COD] Đơn hàng của {order.ShippingName} sẽ thanh toán khi nhận hàng");
            }

            return Task.FromResult("OK");
        }

        // Override hook: gửi thông báo xác nhận đơn hàng
        protected override Task SendConfirmationAsync(Order order)
        {
            Console.WriteLine($"[Thông báo] Đơn hàng #{order.Id} đã được xác nhận!");
            return Task.CompletedTask;
        }
    }
}
