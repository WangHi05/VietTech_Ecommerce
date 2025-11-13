using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using eCommerce.Web.Services;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace eCommerce.Web.Pages.Orders
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly eCommerce.Infrastructure.Data.AppDbContext _context;

        public Order? Order { get; set; }

        // productIds in this order that already have a review by the current user
        public System.Collections.Generic.HashSet<int> ReviewedProductIds { get; set; } = new();

        public DetailsModel(IOrderService orderService, ICartService cartService, eCommerce.Infrastructure.Data.AppDbContext context)
        {
            _orderService = orderService;
            _cartService = cartService;
            _context = context;
        }

        public async System.Threading.Tasks.Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _orderService.GetOrderByIdAsync(id);
            if (Order == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Order.UserId != userId) return Forbid();

            // load reviews for this order by the current user (any status)
            try
            {
                var reviewed = await _context.Reviews
                    .AsNoTracking()
                    .Where(r => r.OrderId == id && r.UserId == userId)
                    .Select(r => r.ProductId)
                    .ToListAsync();

                ReviewedProductIds = reviewed != null ? new System.Collections.Generic.HashSet<int>(reviewed) : new System.Collections.Generic.HashSet<int>();
            }
            catch
            {
                // ignore DB errors here; page will still function without review flags
            }
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
