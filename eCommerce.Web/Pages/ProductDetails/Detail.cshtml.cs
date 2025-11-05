using eCommerce.Application.Services;
// using Product = eCommerce.Core.Entities.Product; // Không cần alias nữa
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
// Import trực tiếp namespace chứa lớp Product
using eCommerce.Core.Entities;


namespace eCommerce.Web.Pages.ProductDetails // Namespace đã đổi theo thư mục
{
    public class DetailModel : PageModel
    {
        private readonly IProductService _productService;

        public DetailModel(IProductService productService)
        {
            _productService = productService;
        }

        // Sử dụng tên lớp đầy đủ hoặc using namespace trực tiếp
        public Product Product { get; set; }

        // Id vẫn được binding từ route template "/Product/{id:int}"
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Logic lấy sản phẩm giữ nguyên
            Product = await _productService.GetProductByIdAsync(Id);

            if (Product == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}