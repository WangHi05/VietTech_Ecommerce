using eCommerce.Application.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly AppDbContext _context;

        public IndexModel(IProductService productService, AppDbContext context)
        {
            _productService = productService;
            _context = context;
        }

        public IEnumerable<Product> Products { get; set; } = new List<Product>();

        public class HomeReview
        {
            public int ReviewId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string? ProductImage { get; set; }
            public string? UserName { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; } = string.Empty;
            public System.DateTime CreatedDate { get; set; }
        }

        public List<HomeReview> Reviews { get; set; } = new List<HomeReview>();
        public int PageSize { get; set; } = 4;
        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        public int TotalReviewsCount { get; set; }

        public async Task OnGetAsync()
        {
            var allProducts = await _productService.GetAllProductsAsync();
            Products = allProducts.Take(20);

            // Load latest 4 approved reviews and include product info
            var reviewsQuery = _context.Reviews
                .Where(r => r.Status == "Approved")
                .OrderByDescending(r => r.CreatedDate);

            TotalReviewsCount = await reviewsQuery.CountAsync();

            var reviews = await reviewsQuery.Skip((PageNumber - 1) * PageSize)
                                           .Take(PageSize)
                                           .ToListAsync();

            var prodIds = reviews.Select(r => r.ProductId).Distinct().ToList();
            var prods = await _context.Products.Where(p => prodIds.Contains(p.Id))
                                           .ToDictionaryAsync(p => p.Id, p => new { p.Name, p.ImageUrl });

            foreach (var r in reviews)
            {
                prods.TryGetValue(r.ProductId, out var p);
                Reviews.Add(new HomeReview
                {
                    ReviewId = r.ReviewId,
                    ProductId = r.ProductId,
                    ProductName = p?.Name ?? "Sản phẩm",
                    ProductImage = p?.ImageUrl,
                    UserName = string.IsNullOrEmpty(r.UserName) ? "Khách" : r.UserName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedDate = r.CreatedDate
                });
            }
        }

        public async Task<JsonResult> OnGetReviewsAsync(int pageNumber = 1)
        {
            var pageSize = PageSize;
            var query = _context.Reviews
                .Where(r => r.Status == "Approved")
                .OrderByDescending(r => r.CreatedDate);

            var total = await query.CountAsync();
            var list = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var prodIds = list.Select(r => r.ProductId).Distinct().ToList();
            var prods = await _context.Products.Where(p => prodIds.Contains(p.Id))
                                       .ToDictionaryAsync(p => p.Id, p => new { p.Name, p.ImageUrl });

            var result = list.Select(r => {
                prods.TryGetValue(r.ProductId, out var p);
                return new {
                    reviewId = r.ReviewId,
                    productId = r.ProductId,
                    productName = p?.Name ?? "Sản phẩm",
                    productImage = p?.ImageUrl,
                    userName = string.IsNullOrEmpty(r.UserName) ? "Khách" : r.UserName,
                    rating = r.Rating,
                    comment = r.Comment,
                    createdDate = r.CreatedDate
                };
            }).ToList();

            return new JsonResult(new { total, pageNumber, pageSize, reviews = result });
        }
    }
}