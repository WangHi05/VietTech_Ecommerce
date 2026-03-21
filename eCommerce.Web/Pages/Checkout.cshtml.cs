using eCommerce.Core.Entities;
using eCommerce.Web.Services;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using eCommerce.Application.Services;
using eCommerce.Application.Mediators;
using eCommerce.Application.Strategies.Payment;
using eCommerce.Core.Interfaces;
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
using Microsoft.AspNetCore.Identity;

namespace eCommerce.Web.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;
        private readonly ILoyaltyService _loyaltyService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockService _stockService;
        private readonly AppDbContext _context;
        private readonly ICheckoutFacade _checkoutFacade;
        private readonly IEnumerable<IPaymentStrategy> _paymentStrategies;

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
        public string? CardName { get; set; } = string.Empty;
        [BindProperty]
        public string? CardNumber { get; set; } = string.Empty;
        [BindProperty]
        public string? CardExpiry { get; set; } = string.Empty;
        [BindProperty]
        public string? CardCvc { get; set; } = string.Empty;
        [BindProperty]
        public int PointsToRedeem { get; set; }

        public List<CartItem> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal PointsDiscount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public string? AppliedVoucherCode { get; set; }
        public int AvailablePoints { get; set; }
        public int MaxRedeemablePoints { get; set; }

        public CheckoutModel(
            ICartService cartService,
            IOrderService orderService,
            IConfiguration configuration,
            ILoyaltyService loyaltyService,
            UserManager<ApplicationUser> userManager,
            IStockService stockService,
            AppDbContext context,
            ICheckoutFacade checkoutFacade,
            IEnumerable<IPaymentStrategy> paymentStrategies)
        {
            _cartService     = cartService;
            _orderService    = orderService;
            _configuration   = configuration;
            _loyaltyService  = loyaltyService;
            _userManager     = userManager;
            _stockService    = stockService;
            _context         = context;
            _checkoutFacade  = checkoutFacade;
            _paymentStrategies = paymentStrategies;
        }

        public async Task<IActionResult> OnGetAsync(int pointsToRedeem = 0)
        {
            // Set points to redeem from query parameter
            PointsToRedeem = pointsToRedeem;
            
            var hasCart = await PrepareCartSummaryAsync();
            if (!hasCart)
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToPage("/Cart");
            }

            // Get user's available points
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var loyaltyInfo = await _loyaltyService.GetOrCreateLoyaltyPointAsync(user.Id);
                    if (loyaltyInfo != null)
                    {
                        AvailablePoints = loyaltyInfo.TotalPoints;
                        // Max points that can be redeemed based on order total (50 points = 1,000đ)
                        MaxRedeemablePoints = Math.Min(AvailablePoints, (int)(Total / 1000 * 50));
                    }
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // BƯỚC 1: Tải lại giỏ hàng và tính toán tiền NGAY LẬP TỨC. 
            // Dòng này giải quyết triệt để lỗi đơn hàng 0đ
            await PrepareCartSummaryAsync();

            // BƯỚC 2: Loại bỏ lỗi validation của Thẻ nếu khách không chọn thanh toán bằng thẻ
            // Dòng này giải quyết triệt để lỗi chữ màu đỏ khi mua COD
            if (PaymentMethod != "card")
            {
                ModelState.Remove(nameof(CardName));
                ModelState.Remove(nameof(CardNumber));
                ModelState.Remove(nameof(CardExpiry));
                ModelState.Remove(nameof(CardCvc));
            }

            // BƯỚC 3: Bây giờ mới kiểm tra xem form có hợp lệ hay không
            if (!ModelState.IsValid)
            {
                return Page(); 
            }

            // BƯỚC 4: Kiểm tra giỏ hàng có trống không
            if (Items == null || !Items.Any()) 
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng của bạn đang trống, không thể đặt hàng.");
                return Page();
            }

            // BƯỚC 5: Xử lý tạo đơn hàng
            var checkoutResult = await _checkoutFacade.PlaceOrderAsync(BuildCheckoutRequest(PaymentMethod));
            
            int newOrderId = checkoutResult.OrderId;
            var orderForPayment = await _orderService.GetOrderByIdAsync(newOrderId);
            
            if (orderForPayment == null)
            {
                ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra, không tìm thấy đơn hàng.");
                return Page();
            }

            // BƯỚC 6: Gọi Strategy Pattern
            var strategy = _paymentStrategies.FirstOrDefault(s => s.ProviderName == PaymentMethod);
            if (strategy == null)
            {
                ModelState.AddModelError(string.Empty, "Phương thức thanh toán không hợp lệ.");
                return Page();
            }

            string redirectUrl = await strategy.ExecutePaymentAsync(orderForPayment);
            return Redirect(redirectUrl);
        }

        public Task<IActionResult> OnPostVnPayAsync()
        {
            return ProcessVnPayAsync();
        }

       private async Task<bool> PrepareCartSummaryAsync()
        {
            // 1. Lấy dữ liệu Items từ CartService (Giữ nguyên)
            Items = await _cartService.GetCartAsync();
            if (!Items.Any()) return false;
            
            decimal rawSubTotal = Items.Sum(i => i.Price * i.Quantity);

            // 2. KHỞI TẠO MEDIATOR PATTERN
            var cartComp = new CartComponent();
            var shippingComp = new ShippingComponent();
            var promoComp = new PromotionComponent();
            
            var mediator = new OrderCheckoutMediator(cartComp, shippingComp, promoComp);

            // 3. ĐƯA DỮ LIỆU VÀO ĐỂ MEDIATOR TỰ ĐỘNG ĐIỀU PHỐI VÀ TÍNH TOÁN
            cartComp.SetSubTotal(rawSubTotal);
            shippingComp.SelectShippingMethod(ShippingMethod ?? "standard");
            promoComp.ApplyPoints(PointsToRedeem);
            promoComp.ApplyVoucher(Discount); // Discount lấy từ VoucherService nếu có

            // 4. LẤY KẾT QUẢ CUỐI CÙNG XUẤT RA VIEW
            SubTotal = cartComp.SubTotal;
            ShippingFee = shippingComp.Fee;
            PointsDiscount = promoComp.PointsDiscount;
            Total = mediator.FinalTotal; // Đã được xử lý ngầm qua Mediator + Decorator

            return true;
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

            var vnpReq = BuildCheckoutRequest("VNPay");
            vnpReq.InitialPaymentStatus = "Đã thanh toán";
            vnpReq.InitialStatus        = "Hoàn tất";

            var vnpFacadeResult = await _checkoutFacade.PlaceOrderAsync(vnpReq);
            if (!vnpFacadeResult.Success)
            {
                ModelState.AddModelError(string.Empty, vnpFacadeResult.ErrorMessage!);
                return Page();
            }
            var id = vnpFacadeResult.OrderId;

            var vnPayConfig = _configuration.GetSection("VnPay");
            var tmnCode = vnPayConfig["TmnCode"];
            var hashSecret = vnPayConfig["HashSecret"];
            var vnpUrl = vnPayConfig["BaseUrl"];
            var returnUrl = vnPayConfig["ReturnUrl"];

            if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(vnpUrl))
            {
                TempData["Warning"] = "VNPay chưa được cấu hình.";
                await _cartService.ClearCartAsync();
                return RedirectToPage("/Payment/Result", new { orderId = id, success = false, reason = "VNPay chưa được cấu hình." });
            }

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = $"{Request.Scheme}://{Request.Host}/Payment/Result";
            }

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
            
            if (hashData.Length > 0)
            {
                hashData.Length -= 1;
            }

            string secureHash;
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(hashData.ToString()));
                secureHash = string.Concat(hashBytes.Select(b => b.ToString("x2")));
            }

            var queryString = hashData.ToString();
            var redirectUrl = $"{vnpUrl}?{queryString}&vnp_SecureHash={secureHash}";

            return Redirect(redirectUrl);
        }

        private CheckoutRequest BuildCheckoutRequest(string paymentMethod) => new()
        {
            PaymentMethod    = paymentMethod,
            ShippingName     = ShippingName,
            ShippingAddress  = ShippingAddress,
            ShippingCountry  = Country,
            ShippingProvince = Province,
            ShippingMethod   = ShippingMethod,
            UserId           = User.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName         = User.Identity?.Name ?? "Guest",
            PointsToRedeem   = PointsToRedeem,
            Items            = Items,
            SubTotal         = SubTotal,
            Discount         = Discount,
            PointsDiscount   = PointsDiscount,
            ShippingFee      = ShippingFee,
            Total            = Total,
            VoucherCode      = AppliedVoucherCode
        };
    }
}
