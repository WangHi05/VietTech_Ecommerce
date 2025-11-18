using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eCommerce.Web.Areas.Admin.Pages.Stock
{
    [Authorize(Roles = "Admin")]
    public class LowStockModel : PageModel
    {
        private readonly IStockService _stockService;

        public LowStockModel(IStockService stockService)
        {
            _stockService = stockService;
        }

        public List<Product> LowStockProducts { get; set; } = new();

        public async Task OnGetAsync()
        {
            LowStockProducts = await _stockService.GetLowStockProducts();
        }
    }
}
