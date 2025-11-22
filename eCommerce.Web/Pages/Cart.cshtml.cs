using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using eCommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace eCommerce.Web.Pages
{
    public class CartModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly AppDbContext _context;

        public List<CartItem> Items { get; set; } = new();
        public List<UserVoucher> UserVouchers { get; set; } = new();
        
        [BindProperty]
        public int UserVoucherId { get; set; }
        
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public int? AppliedUserVoucherId { get; set; }

        public CartModel(ICartService cartService, AppDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Items = await _cartService.GetCartAsync();
            SubTotal = Items.Sum(i => i.Price * i.Quantity);

            // Load user's saved vouchers
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // determine user's region: prefer profile.Vung, fallback to cookie
                var userRegion = Request.Cookies["region"] ?? "Toàn quốc";
                var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (dbUser != null && !string.IsNullOrEmpty(dbUser.Vung))
                {
                    userRegion = dbUser.Vung;
                }

                // Load candidate user vouchers from DB (active / within date)
                var candidate = await _context.UserVouchers
                    .Include(uv => uv.Voucher)
                    .Where(uv => uv.UserId == userId
                        && !uv.IsUsed
                        && uv.Voucher.IsActive
                        && uv.Voucher.ExpiryDate > DateTime.Now
                        && uv.Voucher.StartDate <= DateTime.Now)
                    .ToListAsync();

                // Filter by voucher.Vung in memory (split may not translate to SQL)
                UserVouchers = candidate
                    .Where(uv => string.IsNullOrEmpty(uv.Voucher.Vung)
                        || uv.Voucher.Vung.Trim().Equals("Toàn quốc", System.StringComparison.OrdinalIgnoreCase)
                        || (userRegion != null && uv.Voucher.Vung.Split(',').Select(s => s.Trim()).Contains(userRegion, StringComparer.OrdinalIgnoreCase)))
                    .ToList();
            }

            // Check if voucher is applied (from session)
            var appliedId = HttpContext.Session.GetInt32("AppliedUserVoucherId");
            if (appliedId.HasValue)
            {
                AppliedUserVoucherId = appliedId.Value;
                var userVoucher = UserVouchers.FirstOrDefault(uv => uv.Id == appliedId.Value);
                if (userVoucher != null)
                {
                    Discount = CalculateDiscount(userVoucher.Voucher, SubTotal);
                }
            }

            Total = SubTotal - Discount;
        }

        private decimal CalculateDiscount(Voucher voucher, decimal subTotal)
        {
            // Check minimum order value
            if (subTotal < voucher.MinOrderValue)
                return 0m;

            decimal discount = 0m;

            if (voucher.DiscountPercent.HasValue)
            {
                discount = subTotal * voucher.DiscountPercent.Value / 100;
                
                // Apply max discount cap if set
                if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
                {
                    discount = voucher.MaxDiscountAmount.Value;
                }
            }
            else if (voucher.DiscountAmount.HasValue)
            {
                discount = voucher.DiscountAmount.Value;
            }

            return Math.Round(discount, 0);
        }

        public async Task<IActionResult> OnPostAddAsync(int productId, int qty = 1)
        {
            await _cartService.AddToCartAsync(productId, qty);

            // If this is an AJAX request (fetch from client), return JSON with updated count
            if (Request.Headers != null && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var items = await _cartService.GetCartAsync();
                var count = items?.Sum(i => i.Quantity) ?? 0;
                return new JsonResult(new { success = true, count });
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int productId)
        {
            await _cartService.RemoveFromCartAsync(productId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int productId, int quantity)
        {
            await _cartService.UpdateQuantityAsync(productId, quantity);

            // If AJAX, return updated totals and line total so the client can update without reload
            if (Request.Headers != null && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var items = await _cartService.GetCartAsync();
                var sub = items.Sum(i => i.Price * i.Quantity);
                
                var discount = 0m;
                var appliedId = HttpContext.Session.GetInt32("AppliedUserVoucherId");
                if (appliedId.HasValue && User.Identity?.IsAuthenticated == true)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var userVoucher = await _context.UserVouchers
                        .Include(uv => uv.Voucher)
                        .FirstOrDefaultAsync(uv => uv.Id == appliedId.Value && uv.UserId == userId);
                    
                    if (userVoucher != null)
                    {
                        discount = CalculateDiscount(userVoucher.Voucher, sub);
                    }
                }
                
                var total = sub - discount;
                var productLine = items.FirstOrDefault(i => i.ProductId == productId);
                var lineTotal = productLine != null ? (productLine.Price * productLine.Quantity) : 0m;
                var count = items.Sum(i => i.Quantity);
                
                return new JsonResult(new {
                    success = true,
                    productId,
                    lineTotal = lineTotal.ToString("N0") + " ₫",
                    subtotal = sub.ToString("N0") + " ₫",
                    discount = discount.ToString("N0") + " ₫",
                    total = total.ToString("N0") + " ₫",
                    count
                });
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApplyVoucherAsync()
        {
            var isAjax = Request?.Headers != null && Request.Headers.TryGetValue("X-Requested-With", out var _h) && _h == "XMLHttpRequest";

            if (User.Identity?.IsAuthenticated != true)
            {
                var msg = "Vui lòng đăng nhập để sử dụng voucher.";
                if (isAjax) return new JsonResult(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToPage();
            }

            // Ensure we have UserVoucherId (fallback to form value if model binding failed)
            if (UserVoucherId == 0 && Request?.Form != null && Request.Form.ContainsKey("UserVoucherId"))
            {
                int.TryParse(Request.Form["UserVoucherId"].ToString(), out var parsedId);
                UserVoucherId = parsedId;
            }

            if (UserVoucherId == 0)
            {
                var msg = "Vui lòng chọn voucher trước khi áp dụng.";
                if (isAjax) return new JsonResult(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToPage();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userVoucher = await _context.UserVouchers
                .Include(uv => uv.Voucher)
                .FirstOrDefaultAsync(uv => uv.Id == UserVoucherId
                    && uv.UserId == userId
                    && !uv.IsUsed);

            if (userVoucher == null)
            {
                var msg = "Voucher không hợp lệ.";
                if (isAjax) return new JsonResult(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToPage();
            }

            // Validate voucher
            if (userVoucher.Voucher.ExpiryDate < DateTime.Now)
            {
                var msg = "Voucher đã hết hạn.";
                if (isAjax) return new JsonResult(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToPage();
            }

            if (!userVoucher.Voucher.IsActive)
            {
                var msg = "Voucher không còn khả dụng.";
                if (isAjax) return new JsonResult(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToPage();
            }

            // Validate voucher region (Vung)
            var userRegionForApply = Request.Cookies["region"] ?? "Toàn quốc";
            var dbUserForApply = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (dbUserForApply != null && !string.IsNullOrEmpty(dbUserForApply.Vung))
            {
                userRegionForApply = dbUserForApply.Vung;
            }

            var appliesRegion = string.IsNullOrEmpty(userVoucher.Voucher.Vung)
                || userVoucher.Voucher.Vung.Trim().Equals("Toàn quốc", System.StringComparison.OrdinalIgnoreCase)
                || (userRegionForApply != null && userVoucher.Voucher.Vung.Split(',').Select(s => s.Trim()).Contains(userRegionForApply, StringComparer.OrdinalIgnoreCase));

            if (!appliesRegion)
            {
                var msg = "Voucher này không áp dụng cho vùng của bạn.";
                if (isAjax) return new JsonResult(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToPage();
            }

            var items = await _cartService.GetCartAsync();
            var subTotal = items.Sum(i => i.Price * i.Quantity);

            if (subTotal < userVoucher.Voucher.MinOrderValue)
            {
                var msg = $"Đơn hàng phải từ {userVoucher.Voucher.MinOrderValue:N0}₫ để sử dụng voucher này.";
                if (isAjax) return new JsonResult(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToPage();
            }

            // Apply voucher
            HttpContext.Session.SetInt32("AppliedUserVoucherId", UserVoucherId);
            try
            {
                await _cartService.ApplyVoucherAsync(userVoucher.Voucher.Code);
            }
            catch
            {
                // non-fatal
            }

            var successMsg = $"Đã áp dụng voucher {userVoucher.Voucher.Code}!";
            TempData["Message"] = successMsg;

            if (isAjax)
            {
                var discount = CalculateDiscount(userVoucher.Voucher, subTotal);
                var total = subTotal - discount;
                var count = items.Sum(i => i.Quantity);
                return new JsonResult(new {
                    success = true,
                    message = successMsg,
                    subtotal = subTotal.ToString("N0") + " ₫",
                    discount = (-discount).ToString("N0") + " ₫",
                    total = total.ToString("N0") + " ₫",
                    count
                });
            }

            return RedirectToPage();
        }
    }
}
