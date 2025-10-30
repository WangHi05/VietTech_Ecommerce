using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using eCommerce.Web.Services;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace eCommerce.Web.Pages.Orders
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public Order? Order { get; set; }

        public DetailsModel(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        public async System.Threading.Tasks.Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _orderService.GetOrderByIdAsync(id);
            if (Order == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Order.UserId != userId) return Forbid();
            return Page();
        }

        public async System.Threading.Tasks.Task<IActionResult> OnPostAsync(int id)
        {
            // Reorder action: load the order and replace the cart
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (order.UserId != userId) return Forbid();

            var cartItems = order.Items.Select(i => new CartItem
            {
                ProductId = i.ProductId,
                Name = i.Name,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = string.Empty
            }).ToList();

            await _cartService.SetCartAsync(cartItems);

            return RedirectToPage("/Cart");
        }
    }
}
