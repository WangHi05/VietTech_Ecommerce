using eCommerce.Application.Services;
using eCommerce.Application.Flyweights;
using eCommerce.Application.ViewModels;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        private readonly BrandFlyweightFactory _flyweightFactory;

        public ProductsController(IProductService productService, BrandFlyweightFactory flyweightFactory)
        {
            _productService = productService;
            _flyweightFactory = flyweightFactory;
        }
        // GET: api/Products?categoryId=1 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] int? categoryId)
        {
            var products = await _productService.GetAllProductsAsync();
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }
            return Ok(products);
        }

        // GET: api/Products/5 
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // POST: api/Products 
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            await _productService.CreateProductAsync(product);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        // GET: api/Products/Suggestions?term=lap
        [HttpGet("Suggestions")]
        public async Task<ActionResult<IEnumerable<string>>> GetSuggestions([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2) // Chỉ gợi ý khi có ít nhất 2 ký tự
            {
                return Ok(new List<string>()); // Trả về danh sách rỗng
            }

            var allProducts = await _productService.GetAllProductsAsync();

            var suggestions = allProducts
                .Where(p => p.Name.Contains(term, System.StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Name) // Chỉ lấy tên sản phẩm
                .Take(5); // Giới hạn 5 gợi ý

            return Ok(suggestions);
        }

        //-----------------------Prototype---------------------------
        [HttpPost("{id}/duplicate")]
        public async Task<ActionResult> DuplicateProduct(int id)
        {
            // Lấy sản phẩm từ Service
            var originalProduct = await _productService.GetProductByIdAsync(id);
            if (originalProduct == null) return NotFound();

            // Sử dụng Deep Copy từ Prototype Pattern
            var clonedProduct = (Product)originalProduct.Clone();

            // Tùy chỉnh tên để phân biệt
            clonedProduct.Name = $"[COPY] {originalProduct.Name}";

            // Lưu sản phẩm mới
            await _productService.CreateProductAsync(clonedProduct);

            return Ok(new { id = clonedProduct.Id, message = "Nhân bản sâu thông số thành công!" });
        }
        //-----------------------Prototype---------------------------

        //----------------------Flyweight-----------------
        [HttpGet("OptimizedList")]
        public async Task<ActionResult> GetOptimizedProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            var viewModels = new List<ProductViewModel>();

            foreach (var p in products)
            {
                string brandName = p.Brand?.Name ?? "No Brand";
                string logoUrl = $"/images/brands/{brandName.ToLower()}.png";
                string warranty = "Bảo hành tiêu chuẩn 12 tháng";

                // Lấy Flyweight từ Factory
                var sharedBrandInfo = _flyweightFactory.GetBrandFlyweight(brandName, logoUrl, warranty);

                var vm = new ProductViewModel(p.Id, p.Name, p.Price, p.ImageUrl, sharedBrandInfo);
                viewModels.Add(vm);
            }

            // BƯỚC MỚI: Đóng gói cả "Dữ liệu" lẫn "Thống kê" vào một Object trả về
            var result = new 
            {
                ThongKeToiUuFlyweight = new 
                {
                    TongSoSanPhamLoadLen = viewModels.Count,
                    SoObjectThuongHieuThucTeTrongRAM = _flyweightFactory.GetCacheSize(),
                    SoObjectTietKiemDuoc = viewModels.Count - _flyweightFactory.GetCacheSize()
                },
                DanhSachSanPham = viewModels
            };

            // Trả về thẳng JSON
            return Ok(result);
        }
    }
}
