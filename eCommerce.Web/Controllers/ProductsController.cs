using eCommerce.Application.Services;
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

        public ProductsController(IProductService productService)
        {
            _productService = productService;
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
    }
}
