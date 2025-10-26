using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            // Lấy tất cả sản phẩm và hiển thị 4 sản phẩm đầu tiên làm sản phẩm nổi bật
            var allProducts = await _productService.GetAllProductsAsync();
            Products = allProducts.Take(4); 
        }
    }
}