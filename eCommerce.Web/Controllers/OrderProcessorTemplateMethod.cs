using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Web.Controllers
{
    // TEMPLATE METHOD PATTERN: Định nghĩa bộ khung xử lý đơn hàng
    public abstract class OrderProcessorTemplateMethod : Controller
    {
        protected readonly IOrderRepository _orderRepository;
        protected readonly IProductRepository _productRepository;

        protected OrderProcessorTemplateMethod(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        // Template Method: Bộ khung cố định các bước xử lý đơn hàng
        public async Task<IActionResult> PlaceOrderAsync(Order order)
        {
            var error = ValidateOrder(order);
            if (error != null)
                return BadRequest(new { message = error });

            bool stockOk = await CheckStockAsync(order);
            if (!stockOk)
                return BadRequest(new { message = "Sản phẩm không đủ hàng" });

            var paymentResult = await ProcessPaymentAsync(order);
            if (paymentResult != "OK")
                return BadRequest(new { message = $"Lỗi thanh toán: {paymentResult}" });

            await _orderRepository.AddAsync(order);
            await SendConfirmationAsync(order);

            return Ok(new { message = "Đặt hàng thành công!", orderId = order.Id });
        }

        protected string? ValidateOrder(Order order)
        {
            if (order == null) return "Đơn hàng không hợp lệ";
            if (order.Items == null || order.Items.Count == 0) return "Giỏ hàng trống";
            if (order.Total <= 0) return "Tổng tiền không hợp lệ";
            return null;
        }

        protected async Task<bool> CheckStockAsync(Order order)
        {
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null || product.StockQuantity < item.Quantity)
                    return false;
            }
            return true;
        }

        // Abstract method: Subclass phải implement
        protected abstract Task<string> ProcessPaymentAsync(Order order);

        // Hook: Subclass có thể override
        protected virtual Task SendConfirmationAsync(Order order)
        {
            return Task.CompletedTask;
        }
    }
}
