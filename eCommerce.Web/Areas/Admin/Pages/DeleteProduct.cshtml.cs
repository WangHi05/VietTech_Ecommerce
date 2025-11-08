using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace eCommerce.Web.Areas.Admin.Pages
{
    public class DeleteProductModel : PageModel
    {
        private readonly IProductRepository _productRepo;

        public DeleteProductModel(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        [BindProperty] // Bind để dùng trong <input hidden>
        public Product Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Tải sản phẩm (với Category/Brand) để hiển thị
            Product = await _productRepo.GetByIdAsync(id);

            if (Product == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await _productRepo.DeleteAsync(id);

            TempData["success"] = "Đã xóa sản phẩm thành công!";
            return RedirectToPage("./Index");
        }
    }
}