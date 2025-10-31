using eCommerce.Application.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce.Core.Entities;

namespace eCommerce.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public IEnumerable<Product> Products { get; set; } = new List<Product>();

        public async Task OnGetAsync()
        {
            var allProducts = await _productService.GetAllProductsAsync();
            Products = allProducts.Take(20);
        }
    }
}