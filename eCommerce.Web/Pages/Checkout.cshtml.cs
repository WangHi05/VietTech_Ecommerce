using eCommerce.Core.Entities;
using eCommerce.Web.Services;
using eCommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Security.Claims;

namespace eCommerce.Web.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        [BindProperty]
        public string ShippingName { get; set; } = string.Empty;
        [BindProperty]
        public string ShippingAddress { get; set; } = string.Empty;
        [BindProperty]
        public string Country { get; set; } = string.Empty;
        [BindProperty]
        public string Province { get; set; } = string.Empty;

        public List<CartItem> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }

        public CheckoutModel(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        public async Task OnGetAsync()
        {
            Items = await _cartService.GetCartAsync();
            SubTotal = Items.Sum(i => i.Price * i.Quantity);
            var applied = await _cartService.GetAppliedVoucherAsync();
            if (!string.IsNullOrEmpty(applied))
            {
                Discount = await new VoucherService().GetDiscountAmountAsync(applied, SubTotal);
            }
            ShippingFee = await _cartService.GetShippingAsync() ?? 0m;
            Total = SubTotal - Discount + ShippingFee;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Items = await _cartService.GetCartAsync();
            if (!Items.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToPage("/Cart");
            }

            SubTotal = Items.Sum(i => i.Price * i.Quantity);
            var applied = await _cartService.GetAppliedVoucherAsync();
            if (!string.IsNullOrEmpty(applied))
            {
                // reuse VoucherService from Web project
                var voucherSvc = new VoucherService();
                Discount = await voucherSvc.GetDiscountAmountAsync(applied, SubTotal);
            }
            ShippingFee = await _cartService.GetShippingAsync() ?? 0m;
            Total = SubTotal - Discount + ShippingFee;

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
                VoucherCode = applied,
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

            var id = await _orderService.PlaceOrderAsync(order);

            await _cartService.ClearCartAsync();

            return RedirectToPage("/CheckoutSuccess", new { id });
        }
    }
}
