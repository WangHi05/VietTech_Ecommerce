using eCommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce.Core.Entities;

namespace eCommerce.Web.Pages
{
    public class SearchModel : PageModel
    {
        private readonly IProductService _productService;

        public SearchModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; } = string.Empty;

        // Danh sách kết quả tìm kiếm chính xác
        public IEnumerable<Product> SearchResults { get; set; } = new List<Product>();

        // Danh sách sản phẩm đề xuất/liên quan
        public IEnumerable<Product> RelatedProducts { get; set; } = new List<Product>();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(Query))
            {
                // Lấy tất cả sản phẩm (Lưu ý: Nếu dữ liệu lớn nên lọc từ DB thay vì lấy hết về RAM)
                var allProducts = await _productService.GetAllProductsAsync();

                // 1. Tìm sản phẩm khớp từ khóa (Kết quả chính)
                SearchResults = allProducts
                    .Where(p => p.Name.Contains(Query, System.StringComparison.OrdinalIgnoreCase) ||
                                p.Description.Contains(Query, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // 2. Tìm sản phẩm liên quan (cùng Category với các sản phẩm tìm thấy)
                if (SearchResults.Any())
                {
                    // Lấy danh sách các CategoryId từ các sản phẩm tìm thấy
                    // Giả sử Product có thuộc tính CategoryId
                    var categoryIds = SearchResults.Select(p => p.CategoryId).Distinct().ToList();

                    RelatedProducts = allProducts
                        .Where(p => categoryIds.Contains(p.CategoryId) // Cùng danh mục
                                    && !SearchResults.Contains(p))     // Loại trừ sản phẩm đã hiện ở kết quả chính
                        .OrderBy(x => System.Guid.NewGuid())           // (Tùy chọn) Random để thay đổi đề xuất mỗi lần
                        .Take(4)                                       // Chỉ lấy 4 sản phẩm đề xuất
                        .ToList();
                }
            }
        }
    }
}