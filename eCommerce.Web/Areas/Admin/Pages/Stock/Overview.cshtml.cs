using eCommerce.Application.Composites;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace eCommerce.Web.Areas.Admin.Pages.Stock
{
     [Authorize(Roles = "Admin")]
    public class OverviewModel : PageModel
    {
        private readonly AppDbContext _context;

        public OverviewModel(AppDbContext context)
        {
            _context = context;
        }

        // Biến lưu trữ mã HTML của cây danh mục
        public string HtmlInventoryTree { get; set; } = string.Empty;
        
        // Biến lưu tổng tồn kho toàn siêu thị
        public int TotalStoreStock { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Lấy dữ liệu Category và các Product bên trong từ CSDL
            var categories = await _context.Categories
                                           .Include(c => c.Products)
                                           .ToListAsync();

            // 2. Khởi tạo Node gốc của mẫu Composite
            var rootCuaHang = new CategoryComposite("Toàn bộ kho hàng VIETTECH");

            // 3. Lắp ráp cây đệ quy
            foreach (var category in categories)
            {
                var branch = new CategoryComposite(category.Name);

                foreach (var product in category.Products)
                {
                    // Lấy tên và số lượng tồn kho (StockQuantity)
                    var leaf = new ProductLeaf(product.Name, product.StockQuantity); 
                    branch.Add(leaf);
                }

                rootCuaHang.Add(branch);
            }

            // 4. Gọi 1 hàm duy nhất để quét đệ quy toàn bộ CSDL
            TotalStoreStock = rootCuaHang.GetTotalStock(); 
            HtmlInventoryTree = rootCuaHang.GenerateHtmlTree();
        }
    }
}