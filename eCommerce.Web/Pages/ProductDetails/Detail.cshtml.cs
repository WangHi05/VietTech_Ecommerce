using eCommerce.Application.Services;
// using Product = eCommerce.Core.Entities.Product; // Không cần alias nữa
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
// Import trực tiếp namespace chứa lớp Product
using eCommerce.Core.Entities;


namespace eCommerce.Web.Pages.ProductDetails // Namespace đã đổi theo thư mục
{
    public class DetailModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly eCommerce.Infrastructure.Data.AppDbContext _context;

        public DetailModel(IProductService productService, eCommerce.Infrastructure.Data.AppDbContext context)
        {
            _productService = productService;
            _context = context;
        }

        // Sử dụng tên lớp đầy đủ hoặc using namespace trực tiếp
        public Product Product { get; set; } = default!;

        // Reviews to display (approved)
        public System.Collections.Generic.List<eCommerce.Core.Entities.Review> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

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

            // Load approved reviews for this product
            Reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == Id && r.Status == "Approved")
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            // If the current user has submitted a review for this product that is not yet approved,
            // include it so the user can see their own pending review.
            var userId = User?.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var myReviews = await _context.Reviews
                    .AsNoTracking()
                    .Where(r => r.ProductId == Id && r.UserId == userId && r.Status != "Approved")
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                // Append user's non-approved reviews (avoid duplicates)
                foreach (var mr in myReviews)
                {
                    if (!Reviews.Any(r => r.ReviewId == mr.ReviewId))
                    {
                        Reviews.Insert(0, mr);
                    }
                }
            }

            if (Reviews.Any())
            {
                ReviewCount = Reviews.Count;
                AverageRating = Reviews.Where(r => r.Status == "Approved").DefaultIfEmpty().Average(r => r?.Rating ?? 0);
            }

            return Page();
        }
    }
}