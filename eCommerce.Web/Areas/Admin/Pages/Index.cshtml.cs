using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eCommerce.Web.Areas.Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IProductRepository _productRepo;

        public IndexModel(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public IEnumerable<Product> Products { get; set; } = new List<Product>();

        public async Task OnGetAsync()
        {
            Products = await _productRepo.GetAllAsync();
        }
    }
}