using eCommerce.Application.Services;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Category = eCommerce.Core.Entities.Category;
using eCommerce.Core.Entities;

namespace eCommerce.Web.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryRepository _categoryRepository;

        public ProductsModel(IProductService productService, ICategoryRepository categoryRepository)
        {
            _productService = productService;
            _categoryRepository = categoryRepository;
        }

        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _categoryRepository.GetAllAsync();
            var allProducts = await _productService.GetAllProductsAsync();

            if (CategoryId.HasValue && CategoryId.Value > 0)
            {
                Products = allProducts.Where(p => p.CategoryId == CategoryId.Value);
            }
            else
            {
                Products = allProducts;
            }
        }
    }
}