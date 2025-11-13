using eCommerce.Core.Entities;
using eCommerce.Web.Services;
using eCommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eCommerce.Web.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;

        [BindProperty]
    public string ShippingMethod { get; set; } = "standard";
    [BindProperty]
        public string ShippingName { get; set; } = string.Empty;
        [BindProperty]
        public string ShippingAddress { get; set; } = string.Empty;
        [BindProperty]
        public string Country { get; set; } = string.Empty;
        [BindProperty]
        public string Province { get; set; } = string.Empty;
        [BindProperty]
        public string PaymentMethod { get; set; } = "card";
        [BindProperty]
        public string CardName { get; set; } = string.Empty;
        [BindProperty]
        public string CardNumber { get; set; } = string.Empty;
        [BindProperty]
        public string CardExpiry { get; set; } = string.Empty;
        [BindProperty]
        public string CardCvc { get; set; } = string.Empty;

        public List<CartItem> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public string? AppliedVoucherCode { get; set; }

        public CheckoutModel(ICartService cartService, IOrderService orderService, IConfiguration configuration)
        {
            _cartService = cartService;
            _orderService = orderService;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var hasCart = await PrepareCartSummaryAsync();
            if (!hasCart)
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToPage("/Cart");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var hasCart = await PrepareCartSummaryAsync();
            if (!hasCart)
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToPage("/Cart");
            }

            var method = (PaymentMethod ?? string.Empty).Trim().ToLowerInvariant();

            if (method == "card")
            {
                ValidateCardDetails();
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var cardOrder = CreateOrderFromCart();
                cardOrder.PaymentMethod = "Card";
                cardOrder.PaymentStatus = "Pending";
                cardOrder.Status = "Pending";
                cardOrder.CardHolderName = (CardName ?? string.Empty).Trim();
                cardOrder.CardLast4 = ExtractCardLast4(CardNumber);

                var cardOrderId = await _orderService.PlaceOrderAsync(cardOrder);

                return RedirectToPage("/Payment/CardOtp", new { orderId = cardOrderId });
            }

            if (method == "cod")
            {
                var codOrder = CreateOrderFromCart();
                codOrder.PaymentMethod = "COD";
                codOrder.PaymentStatus = "AwaitingPayment";
                codOrder.Status = "Pending";

                var codOrderId = await _orderService.PlaceOrderAsync(codOrder);
                await _cartService.ClearCartAsync();

                return RedirectToPage("/Payment/Result", new { orderId = codOrderId, success = true, method = "cod" });
            }

            if (method == "vnpay")
            {
                return await ProcessVnPayAsync(cartAlreadyPrepared: true);
            }

            ModelState.AddModelError(nameof(PaymentMethod), "Phương thức thanh toán không hợp lệ.");
            return Page();
        }

        // VNPay redirect handler — creates order then redirects to VNPay payment page
        public Task<IActionResult> OnPostVnPayAsync()
        {
            return ProcessVnPayAsync();
        }

        private async Task<bool> PrepareCartSummaryAsync()
        {
            Items = await _cartService.GetCartAsync();
            if (!Items.Any())
            {
                return false;
            }

            SubTotal = Items.Sum(i => i.Price * i.Quantity);

            var applied = await _cartService.GetAppliedVoucherAsync();
            AppliedVoucherCode = applied;
            if (!string.IsNullOrEmpty(applied))
            {
                var voucherSvc = new VoucherService();
                Discount = await voucherSvc.GetDiscountAmountAsync(applied, SubTotal);
            }

            // Simple shipping fee logic: if user selected an explicit method, use mapped fees;
            // otherwise fall back to cart service or 0.
            if (!string.IsNullOrWhiteSpace(ShippingMethod) && ShippingMethod.Equals("express", StringComparison.OrdinalIgnoreCase))
            {
                ShippingFee = 50000m; // express flat fee
            }
            else if (!string.IsNullOrWhiteSpace(ShippingMethod) && ShippingMethod.Equals("pickup", StringComparison.OrdinalIgnoreCase))
            {
                ShippingFee = 0m; // pickup free
            }
            else
            {
                ShippingFee = await _cartService.GetShippingAsync() ?? 0m;
            }
            Total = SubTotal - Discount + ShippingFee;

            return true;
        }

        private Order CreateOrderFromCart()
        {
            var order = new Order
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CreatedAt = DateTime.UtcNow,
                ShippingName = ShippingName,
                ShippingAddress = ShippingAddress,
                ShippingCountry = Country,
                ShippingProvince = Province,
                SubTotal = SubTotal,
                Discount = Discount,
                ShippingFee = ShippingFee,
                VoucherCode = AppliedVoucherCode,
                Total = Total
            };

            foreach (var it in Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = it.ProductId,
                    Name = it.Name,
                    Price = it.Price,
                    Quantity = it.Quantity
                });
            }

            return order;
        }

        private void ValidateCardDetails()
        {
            if (string.IsNullOrWhiteSpace(CardName))
            {
                ModelState.AddModelError(nameof(CardName), "Vui lòng nhập tên chủ thẻ.");
            }

            var digits = new string((CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
            if (string.IsNullOrWhiteSpace(digits) || digits.Length < 13 || digits.Length > 19)
            {
                ModelState.AddModelError(nameof(CardNumber), "Số thẻ không hợp lệ (13-19 chữ số).");
            }

            if (!Regex.IsMatch(CardExpiry ?? string.Empty, @"^(0[1-9]|1[0-2])/\d{2}$"))
            {
                ModelState.AddModelError(nameof(CardExpiry), "Ngày hết hạn không hợp lệ (MM/YY).");
            }
            else
            {
                var parts = (CardExpiry ?? string.Empty).Split('/');
                if (int.TryParse(parts.ElementAtOrDefault(0) ?? string.Empty, out var month)
                    && int.TryParse(parts.ElementAtOrDefault(1) ?? string.Empty, out var yearPart))
                {
                    var year = 2000 + yearPart;
                    var daysInMonth = DateTime.DaysInMonth(year, month);
                    var expiry = new DateTime(year, month, daysInMonth);
                    if (expiry < DateTime.UtcNow.Date)
                    {
                        ModelState.AddModelError(nameof(CardExpiry), "Thẻ đã hết hạn.");
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(CardExpiry), "Ngày hết hạn không hợp lệ.");
                }
            }

            if (!Regex.IsMatch(CardCvc ?? string.Empty, @"^\d{3,4}$"))
            {
                ModelState.AddModelError(nameof(CardCvc), "CVC phải gồm 3-4 chữ số.");
            }
        }

        private static string ExtractCardLast4(string cardNumber)
        {
            var digits = new string((cardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digits))
            {
                return string.Empty;
            }

            var take = Math.Min(4, digits.Length);
            return digits.Substring(digits.Length - take, take);
        }

        private async Task<IActionResult> ProcessVnPayAsync(bool cartAlreadyPrepared = false)
        {
            if (!cartAlreadyPrepared)
            {
                var hasCart = await PrepareCartSummaryAsync();
                if (!hasCart)
                {
                    TempData["Error"] = "Giỏ hàng trống.";
                    return RedirectToPage("/Cart");
                }
            }

            var order = CreateOrderFromCart();
            order.PaymentMethod = "VNPay";
            order.PaymentStatus = "Succeeded";
            order.Status = "Paid";

            var id = await _orderService.PlaceOrderAsync(order);

            var vnPayConfig = _configuration.GetSection("VnPay");
            var tmnCode = vnPayConfig["TmnCode"];
            var hashSecret = vnPayConfig["HashSecret"];
            var vnpUrl = vnPayConfig["BaseUrl"];
            var returnUrl = vnPayConfig["ReturnUrl"];

            if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(vnpUrl))
            {
                TempData["Warning"] = "VNPay chưa được cấu hình. Đơn hàng đã được tạo nhưng không thể thanh toán qua VNPay.";
                await _cartService.ClearCartAsync();
                return RedirectToPage("/Payment/Result", new { orderId = id, success = false, reason = "VNPay chưa được cấu hình." });
            }

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = $"{Request.Scheme}://{Request.Host}/Payment/Result";
            }

            // Lấy IP address
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            if (ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }

            var vnpParams = new SortedList<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_Amount", ((long)(Total * 100)).ToString() },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", ipAddress },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", $"Thanh toan don hang {id}" },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", id.ToString() }
            };

            // Tạo chuỗi hash data
            var hashData = new StringBuilder();
            foreach (var kv in vnpParams)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    hashData.Append(System.Net.WebUtility.UrlEncode(kv.Key));
                    hashData.Append('=');
                    hashData.Append(System.Net.WebUtility.UrlEncode(kv.Value));
                    hashData.Append('&');
                }
            }
            
            // Xóa ký tự & cuối cùng
            if (hashData.Length > 0)
            {
                hashData.Length -= 1;
            }

            // Tính SecureHash
            string secureHash;
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(hashData.ToString()));
                secureHash = string.Concat(hashBytes.Select(b => b.ToString("x2")));
            }

            // Tạo URL redirect
            var queryString = hashData.ToString();
            var redirectUrl = $"{vnpUrl}?{queryString}&vnp_SecureHash={secureHash}";

            await _cartService.ClearCartAsync();

            return Redirect(redirectUrl);
        }
    }
}
