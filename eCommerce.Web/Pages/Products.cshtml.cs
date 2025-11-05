using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce.Web.Pages
{
    // Giả sử tên Model là ProductsModel
    public class ProductsModel : PageModel
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IBrandRepository _brandRepo;

        public ProductsModel(
            IProductRepository productRepo,
            ICategoryRepository categoryRepo,
            IBrandRepository brandRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _brandRepo = brandRepo;
        }

        // === CÁC THUỘC TÍNH BIND TỪ URL ===
        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? BrandId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Color { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Size { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; }

        // === DỮ LIỆU HIỂN THỊ TRANG ===
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Product> Products { get; set; }

        // === DỮ LIỆU CHO BỘ LỌC ===
        public SelectList BrandList { get; set; }
        public List<string> ColorList { get; set; }
        public List<string> SizeList { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Tải danh sách sản phẩm
            Products = await _productRepo.GetFilteredAsync(
                CategoryId,
                BrandId,
                Color,
                Size,
                MinPrice,
                MaxPrice,
                SortBy
            );

            // 2. Tải danh mục cho sidebar
            Categories = await _categoryRepo.GetAllAsync();

            // 3. Tải dữ liệu cho các ô filter
            var brands = await _brandRepo.GetAllAsync();
            BrandList = new SelectList(brands, "Id", "Name", BrandId);

            ColorList = await _productRepo.GetUniqueColorsAsync();
            SizeList = await _productRepo.GetUniqueSizesAsync();
        }
    }
}