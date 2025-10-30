using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using eCommerce.Application.Services;

namespace eCommerce.Web.Pages
{
    public class CheckoutSuccessModel : PageModel
    {
        private readonly IOrderService _orderService;

        public int OrderId { get; set; }

        public CheckoutSuccessModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task OnGetAsync(int id)
        {
            OrderId = id;
            // Could load order details if needed
        }
    }
}
