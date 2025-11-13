using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using eCommerce.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Web.Pages.Orders
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly eCommerce.Infrastructure.Data.AppDbContext _context;

        public List<Order> Orders { get; set; } = new();

        // orders that already have at least one review by this user
        public System.Collections.Generic.HashSet<int> ReviewedOrderIds { get; set; } = new();

        public IndexModel(IOrderService orderService, eCommerce.Infrastructure.Data.AppDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Orders = new List<Order>();
                return;
            }

            Orders = await _orderService.GetOrdersByUserAsync(userId);
            
            // Sắp xếp đơn hàng mới nhất lên trên
            Orders = Orders.OrderByDescending(o => o.CreatedAt).ToList();

            try
            {
                var ids = Orders.Select(o => o.Id).ToList();
                var reviewed = await _context.Reviews
                    .Where(r => r.OrderId.HasValue && ids.Contains(r.OrderId.Value) && r.UserId == userId)
                    .Select(r => r.OrderId)
                    .Distinct()
                    .Where(o => o.HasValue)
                    .Select(o => o!.Value)
                    .ToListAsync();

                ReviewedOrderIds = new System.Collections.Generic.HashSet<int>(reviewed);
            }
            catch
            {
                // ignore DB errors; Orders list still usable
            }
        }
    }
}
