using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using eCommerce.Web.Constants;
using eCommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace eCommerce.Web.Pages.Payment
{
    public class CardOtpModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        [BindProperty]
        public int OrderId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
        public string OtpCode { get; set; } = string.Empty;

        public string? CardDisplay { get; set; }
        public string? CardHolder { get; set; }

        public CardOtpModel(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng cần xác thực.";
                return RedirectToPage("/Checkout");
            }

            OrderId = orderId;
            PopulateCardInfo(order);

            if (!string.Equals(order.PaymentMethod, "Card", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Warning"] = "Đơn hàng không yêu cầu xác thực OTP.";
                return RedirectToPage("/Payment/Result", new 
                { 
                    orderId, 
                    success = string.Equals(order.PaymentStatus, "Succeeded", StringComparison.OrdinalIgnoreCase),
                    method = "card" 
                });
            }

            if (!string.Equals(order.PaymentStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Payment/Result", new 
                { 
                    orderId, 
                    success = string.Equals(order.PaymentStatus, "Succeeded", StringComparison.OrdinalIgnoreCase),
                    method = "card" 
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var order = await _orderService.GetOrderByIdAsync(OrderId);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage("/Checkout");
            }

            PopulateCardInfo(order);

            if (!string.Equals(order.PaymentMethod, "Card", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Warning"] = "Đơn hàng không yêu cầu xác thực OTP.";
                return RedirectToPage("/Payment/Result", new 
                { 
                    orderId = OrderId, 
                    success = string.Equals(order.PaymentStatus, "Succeeded", StringComparison.OrdinalIgnoreCase),
                    method = "card" 
                });
            }

            if (!string.Equals(order.PaymentStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Payment/Result", new 
                { 
                    orderId = OrderId, 
                    success = string.Equals(order.PaymentStatus, "Succeeded", StringComparison.OrdinalIgnoreCase),
                    method = "card" 
                });
            }

            if (!string.Equals(OtpCode?.Trim(), PaymentConstants.SimulatedCardOtp, StringComparison.Ordinal))
            {
                await _orderService.UpdatePaymentStateAsync(OrderId, "Failed", "Failed");
                return RedirectToPage("/Payment/Result", new 
                { 
                    orderId = OrderId, 
                    success = false, 
                    reason = "OTP không chính xác.",
                    method = "card"
                });
            }

            await _orderService.UpdatePaymentStateAsync(OrderId, "Paid", "Succeeded", DateTime.UtcNow);
            await _cartService.ClearCartAsync();

            return RedirectToPage("/Payment/Result", new 
            { 
                orderId = OrderId, 
                success = true,
                method = "card"
            });
        }

        private void PopulateCardInfo(Order order)
        {
            CardHolder = order.CardHolderName;
            if (string.IsNullOrWhiteSpace(order.CardLast4))
            {
                CardDisplay = "**** **** **** ****";
            }
            else
            {
                CardDisplay = $"**** **** **** {order.CardLast4}";
            }
        }
    }
}
