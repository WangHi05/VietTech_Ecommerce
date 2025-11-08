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
using System.Threading.Tasks;

namespace eCommerce.Web.Areas.Admin.Pages
{
    public class AddProductModel : PageModel
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IBrandRepository _brandRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AddProductModel(
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
        [Required(ErrorMessage = "Ảnh sản phẩm là bắt buộc")]
        public IFormFile UploadedImage { get; set; }

        // === CẬP NHẬT: THÊM 2 DANH SÁCH NÀY ===
        // 2 List này sẽ nhận dữ liệu từ các input động
        [BindProperty]
        public List<string> SpecKeys { get; set; } = new();

        [BindProperty]
        public List<string> SpecValues { get; set; } = new();
        // === HẾT CẬP NHẬT ===

        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> BrandList { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDropdownLists();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownLists();
                return Page();
            }

            // === CẬP NHẬT: XỬ LÝ THÔNG SỐ KỸ THUẬT ===
            // Gộp 2 list SpecKeys và SpecValues vào Dictionary
            if (SpecKeys != null && SpecValues != null)
            {
                for (int i = 0; i < SpecKeys.Count; i++)
                {
                    // Chỉ thêm nếu cả key và value đều có nội dung
                    if (i < SpecValues.Count && !string.IsNullOrEmpty(SpecKeys[i]) && !string.IsNullOrEmpty(SpecValues[i]))
                    {
                        // Thêm vào Dictionary của Product
                        Product.Specifications[SpecKeys[i]] = SpecValues[i];
                    }
                }
            }
            // === HẾT CẬP NHẬT ===

            // Xử lý file ảnh
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
                Product.ImageUrl = "/images/products/" + uniqueFileName;
            }

            // Thêm sản phẩm vào CSDL
            await _productRepo.AddAsync(Product);

            TempData["success"] = "Đã thêm sản phẩm thành công!";
            return RedirectToPage("./Index");
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