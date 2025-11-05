using eCommerce.Core.Entities;
using eCommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eCommerce.Web.Pages
{
    public class CartModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IVoucherService _voucherService;

        public List<CartItem> Items { get; set; } = new();
        public Dictionary<string, string> AvailableVouchers { get; set; } = new();
        [BindProperty]
        public string VoucherCode { get; set; } = string.Empty;
        [BindProperty]
        public string Country { get; set; } = string.Empty;
        [BindProperty]
        public string Province { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public List<string> PresetProvinces { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public CartModel(ICartService cartService, IVoucherService voucherService)
        {
            _cartService = cartService;
            _voucherService = voucherService;
        }

        public async Task OnGetAsync()
        {
            Items = await _cartService.GetCartAsync();
            AvailableVouchers = await _voucherService.GetAvailableVouchersAsync();
            // A small list of Vietnamese provinces to choose from
            PresetProvinces = new List<string>
            {
                "Hồ Chí Minh",
                "Hà Nội",
                "Đà Nẵng",
                "Hải Phòng",
                "Cần Thơ",
                "Khác"
            };

            // compute totals
            SubTotal = Items.Sum(i => i.Price * i.Quantity);

            // applied voucher (from session)
            var applied = await _cartService.GetAppliedVoucherAsync();
            if (!string.IsNullOrEmpty(applied))
            {
                Discount = await _voucherService.GetDiscountAmountAsync(applied, SubTotal);
                VoucherCode = applied;
            }
            else
            {
                Discount = 0m;
            }

            var shipping = await _cartService.GetShippingAsync();
            ShippingFee = shipping ?? 0m;

            Total = SubTotal - Discount + ShippingFee;
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
                var applied = await _cartService.GetAppliedVoucherAsync();
                var discount = 0m;
                if (!string.IsNullOrEmpty(applied)) discount = await _voucherService.GetDiscountAmountAsync(applied, sub);
                var shipping = await _cartService.GetShippingAsync() ?? 0m;
                var total = sub - discount + shipping;
                var productLine = items.FirstOrDefault(i => i.ProductId == productId);
                var lineTotal = productLine != null ? (productLine.Price * productLine.Quantity) : 0m;
                var count = items.Sum(i => i.Quantity);
                return new JsonResult(new {
                    success = true,
                    productId,
                    lineTotal = lineTotal.ToString("N0") + " ₫",
                    subtotal = sub.ToString("N0") + " ₫",
                    discount = discount.ToString("N0") + " ₫",
                    shipping = shipping.ToString("N0") + " ₫",
                    total = total.ToString("N0") + " ₫",
                    count
                });
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApplyVoucherAsync()
        {
            if (await _voucherService.ValidateVoucherAsync(VoucherCode))
            {
                await _cartService.ApplyVoucherAsync(VoucherCode);
                TempData["Message"] = "Voucher đã được áp dụng.";
            }
            else
            {
                TempData["Error"] = "Mã voucher không hợp lệ.";
            }

            if (Request.Headers != null && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var items = await _cartService.GetCartAsync();
                var sub = items.Sum(i => i.Price * i.Quantity);
                var applied = await _cartService.GetAppliedVoucherAsync();
                var discount = 0m;
                if (!string.IsNullOrEmpty(applied)) discount = await _voucherService.GetDiscountAmountAsync(applied, sub);
                var shipping = await _cartService.GetShippingAsync() ?? 0m;
                var total = sub - discount + shipping;
                var count = items.Sum(i => i.Quantity);
                return new JsonResult(new {
                    success = true,
                    subtotal = sub.ToString("N0") + " ₫",
                    discount = discount.ToString("N0") + " ₫",
                    shipping = shipping.ToString("N0") + " ₫",
                    total = total.ToString("N0") + " ₫",
                    count
                });
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCalculateShippingAsync()
        {
            ShippingFee = await _cartService.CalculateShippingAsync(Country, Province);
            // persist shipping fee into session so OnGet can compute totals
            await _cartService.SetShippingAsync(ShippingFee);
            TempData["Message"] = $"Phí vận chuyển: {ShippingFee:N0} ₫";

            if (Request.Headers != null && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var items = await _cartService.GetCartAsync();
                var sub = items.Sum(i => i.Price * i.Quantity);
                var applied = await _cartService.GetAppliedVoucherAsync();
                var discount = 0m;
                if (!string.IsNullOrEmpty(applied)) discount = await _voucherService.GetDiscountAmountAsync(applied, sub);
                var shipping = ShippingFee;
                var total = sub - discount + shipping;
                var count = items.Sum(i => i.Quantity);
                return new JsonResult(new {
                    success = true,
                    subtotal = sub.ToString("N0") + " ₫",
                    discount = discount.ToString("N0") + " ₫",
                    shipping = shipping.ToString("N0") + " ₫",
                    total = total.ToString("N0") + " ₫",
                    count
                });
            }

            return RedirectToPage();
        }
    }
}
