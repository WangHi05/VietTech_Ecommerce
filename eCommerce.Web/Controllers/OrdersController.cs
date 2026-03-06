using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Web.Controllers
{
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

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] Order order)
        {
            return await PlaceOrderAsync(order);
        }

        protected override Task<string> ProcessPaymentAsync(Order order)
        {
            if (order.PaymentMethod == "VnPay")
            {
                order.PaymentStatus = "Chờ thanh toán";
                order.Status = "Đang chờ";
                Console.WriteLine($"[VnPay] Tạo link thanh toán cho đơn hàng của {order.ShippingName}");
            }
            else
            {
                order.PaymentStatus = "Chưa thanh toán";
                order.Status = "Đang chờ";
                Console.WriteLine($"[COD] Đơn hàng của {order.ShippingName} sẽ thanh toán khi nhận hàng");
            }

            return Task.FromResult("OK");
        }

        protected override Task SendConfirmationAsync(Order order)
        {
            Console.WriteLine($"[Thông báo] Đơn hàng #{order.Id} đã được xác nhận!");
            return Task.CompletedTask;
        }
    }
}
