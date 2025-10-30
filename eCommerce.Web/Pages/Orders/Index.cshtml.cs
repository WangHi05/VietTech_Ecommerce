using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using eCommerce.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Security.Claims;

namespace eCommerce.Web.Pages.Orders
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;

        public List<Order> Orders { get; set; } = new();

        public IndexModel(IOrderService orderService)
        {
            _orderService = orderService;
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
        }
    }
}
