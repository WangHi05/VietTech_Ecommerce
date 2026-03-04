using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Web.Controllers
{
    /// <summary>
    /// TEMPLATE METHOD PATTERN
    /// Abstract base class kế thừa Controller.
    /// Định nghĩa bộ khung xử lý đơn hàng — các bước cố định, chỉ bước thanh toán là khác nhau.
    /// </summary>
    public abstract class OrderProcessorTemplateMethod : Controller
    {
        protected readonly IOrderRepository _orderRepository;
        protected readonly IProductRepository _productRepository;

        protected OrderProcessorTemplateMethod(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        // ============================================================
        // TEMPLATE METHOD: Bộ khung cố định, không cho override
        // ============================================================
        public async Task<IActionResult> PlaceOrderAsync(Order order)
        {
            // Bước 1: Validate đơn hàng (chung)
            var error = ValidateOrder(order);
            if (error != null)
                return BadRequest(new { message = error });

            // Bước 2: Kiểm tra tồn kho (chung)
            bool stockOk = await CheckStockAsync(order);
            if (!stockOk)
                return BadRequest(new { message = "Sản phẩm không đủ hàng" });

            // Bước 3: Xử lý thanh toán (KHÁC NHAU → abstract, subclass tự làm)
            var paymentResult = await ProcessPaymentAsync(order);
            if (paymentResult != "OK")
                return BadRequest(new { message = $"Lỗi thanh toán: {paymentResult}" });

            // Bước 4: Lưu đơn hàng vào DB (chung)
            await _orderRepository.AddAsync(order);

            // Bước 5: Gửi thông báo (có thể override, mặc định không làm gì)
            await SendConfirmationAsync(order);

            return Ok(new { message = "Đặt hàng thành công!", orderId = order.Id });
        }

        // ============================================================
        // Bước CHUNG — có sẵn, dùng chung cho tất cả
        // ============================================================
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

        // ============================================================
        // Bước KHÁC NHAU — subclass BẮT BUỘC phải implement
        // ============================================================
        protected abstract Task<string> ProcessPaymentAsync(Order order);

        // ============================================================
        // Hook — subclass CÓ THỂ override nếu muốn thêm thông báo
        // ============================================================
        protected virtual Task SendConfirmationAsync(Order order)
        {
            return Task.CompletedTask;
        }
    }
}
