using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq; // <-- Phải có using này
using System.Threading.Tasks;

namespace eCommerce.Web.Areas.Admin.Pages
{
    public class EditProductModel : PageModel
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IBrandRepository _brandRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EditProductModel(
            IProductRepository productRepo,
            ICategoryRepository categoryRepo,
            IBrandRepository brandRepo,
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _brandRepo = brandRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        [Display(Name = "Ảnh Sản Phẩm")]
        public IFormFile? UploadedImage { get; set; } // Cho phép null (không bắt buộc)

        [BindProperty]
        public List<string> SpecKeys { get; set; } = new();

        [BindProperty]
        public List<string> SpecValues { get; set; } = new();

        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> BrandList { get; set; }
        
        // Dùng để hiển thị ảnh cũ nếu không đổi ảnh mới
        public string ExistingImageUrl { get; set; }

        // OnGet: Tải sản phẩm lên form
        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Tải sản phẩm từ CSDL
            Product = await _productRepo.GetByIdAsync(id);

            if (Product == null)
            {
                return NotFound();
            }

            // Tải danh sách dropdowns
            await LoadDropdownLists();
            
            // Lưu lại URL ảnh cũ để hiển thị
            ExistingImageUrl = Product.ImageUrl;

            // Tải Specifications vào 2 list SpecKeys/SpecValues
            if (Product.Specifications != null && Product.Specifications.Count > 0)
            {
                SpecKeys = Product.Specifications.Keys.ToList();
                SpecValues = Product.Specifications.Values.ToList();
            }

            return Page();
        }

        // OnPost: Lưu thay đổi
        public async Task<IActionResult> OnPostAsync(int id)
        {
            // Không cần kiểm tra ModelState.IsValid ngay, vì Product có thể không hợp lệ
            // Tải lại dropdowns phòng trường hợp trả về Page()
            await LoadDropdownLists();

            // Lấy sản phẩm gốc từ CSDL
            var productToUpdate = await _productRepo.GetByIdAsync(id);
            if (productToUpdate == null)
            {
                return NotFound();
            }

            // Xử lý ảnh (Nếu user tải ảnh mới)
            if (UploadedImage != null)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + UploadedImage.FileName;
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                Directory.CreateDirectory(uploadsFolder);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadedImage.CopyToAsync(fileStream);
                }
                productToUpdate.ImageUrl = "/images/products/" + uniqueFileName;
            }
            // Nếu không tải ảnh mới, ImageUrl gốc sẽ được giữ nguyên

            // Cập nhật các trường từ form (Product)
            // Dùng TryUpdateModelAsync để cập nhật an toàn các trường
            if (await TryUpdateModelAsync<Product>(
                productToUpdate,
                "Product", // Tên của [BindProperty]
                p => p.Name, p => p.Description, p => p.Price, p => p.StockQuantity,
                p => p.CategoryId, p => p.BrandId, p => p.Color, p => p.Size))
            {
                // Xử lý Specifications (Xóa cũ, thêm mới)
                productToUpdate.Specifications.Clear();
                if (SpecKeys != null && SpecValues != null)
                {
                    for (int i = 0; i < SpecKeys.Count; i++)
                    {
                        if (i < SpecValues.Count && !string.IsNullOrEmpty(SpecKeys[i]) && !string.IsNullOrEmpty(SpecValues[i]))
                        {
                            productToUpdate.Specifications[SpecKeys[i]] = SpecValues[i];
                        }
                    }
                }

                // Gọi hàm Update
                await _productRepo.UpdateAsync(productToUpdate);

                TempData["success"] = "Đã cập nhật sản phẩm thành công!";
                return RedirectToPage("./Index");
            }

            // Nếu TryUpdateModelAsync thất bại (lỗi validation), hiển thị lại trang
            return Page();
        }

        private async Task LoadDropdownLists()
        {
            var categories = await _categoryRepo.GetAllAsync();
            CategoryList = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });

            var brands = await _brandRepo.GetAllAsync();
            BrandList = brands.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            });
        }
    }
}