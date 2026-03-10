using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Application.DesignPatterns.Iterator;
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

        // Iterator Pattern - duyệt qua orders theo status
        [HttpGet("by-status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            var allOrders = await _orderRepository.GetAllForAdminAsync();

            // Tạo OrderCollection và thêm orders vào
            var orderCollection = new OrderCollection();
            foreach (var order in allOrders)
            {
                orderCollection.AddOrder(order);
            }

            // Sử dụng Iterator để duyệt qua orders theo status
            var iterator = orderCollection.CreateStatusIterator(status);
            var filteredOrders = new List<Order>();

            while (iterator.MoveNext())
            {
                var order = iterator.Current;
                if (order != null)
                {
                    filteredOrders.Add(order);
                }
            }

            return Ok(new
            {
                status = status,
                count = filteredOrders.Count,
                orders = filteredOrders
            });
        }

        // Iterator Pattern - duyệt qua orders theo payment status
        [HttpGet("by-payment-status/{paymentStatus}")]
        public async Task<IActionResult> GetOrdersByPaymentStatus(string paymentStatus)
        {
            var allOrders = await _orderRepository.GetAllForAdminAsync();

            var orderCollection = new OrderCollection();
            foreach (var order in allOrders)
            {
                orderCollection.AddOrder(order);
            }

            var iterator = orderCollection.CreatePaymentIterator(paymentStatus);
            var filteredOrders = new List<Order>();

            while (iterator.MoveNext())
            {
                var order = iterator.Current;
                if (order != null)
                {
                    filteredOrders.Add(order);
                }
            }

            return Ok(new
            {
                paymentStatus = paymentStatus,
                count = filteredOrders.Count,
                orders = filteredOrders
            });
        }
    }
}
