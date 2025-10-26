using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce.Web.Pages
{
    public class SearchModel : PageModel
    {
        private readonly IProductService _productService;

        public SearchModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; } = string.Empty;

        public IEnumerable<Product> SearchResults { get; set; } = new List<Product>();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(Query))
            {
                var allProducts = await _productService.GetAllProductsAsync();
                SearchResults = allProducts
                    .Where(p => p.Name.Contains(Query, System.StringComparison.OrdinalIgnoreCase) || 
                                p.Description.Contains(Query, System.StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
