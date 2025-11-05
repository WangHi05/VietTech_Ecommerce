using eCommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace eCommerce.Web.Pages.Payment
{
    public class PaymentResultModel : PageModel
    {
        private readonly IOrderService _orderService;

        public int OrderId { get; set; }
        public bool Success { get; set; }
        public string? Reason { get; set; }
        public string? PaymentStatus { get; set; }
        public string? Method { get; set; }

        public PaymentResultModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OnGetAsync(int orderId, bool success, string? method = null, string? reason = null)
        {
            OrderId = orderId;
            Success = success;
            Reason = reason;
            Method = method;

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage("/Checkout");
            }

            PaymentStatus = order.PaymentStatus;
            return Page();
        }
    }
}
