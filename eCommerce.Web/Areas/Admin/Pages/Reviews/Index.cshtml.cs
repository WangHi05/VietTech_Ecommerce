using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Web.Areas.Admin.Pages.Reviews
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public class ReviewView
        {
            public int ReviewId { get; set; }
            public int ProductId { get; set; }
            public string? ProductName { get; set; }
            public string? UserId { get; set; }
            public string? UserName { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; } = string.Empty;
            public System.DateTime CreatedDate { get; set; }
            public string Status { get; set; } = "Pending";
        }

        public List<ReviewView> Reviews { get; set; } = new List<ReviewView>();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 15;

        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true, Name = "filterProductId")]
        public int? FilterProductId { get; set; }

        [BindProperty(SupportsGet = true, Name = "filterStatus")]
        public string? FilterStatus { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Reviews.AsQueryable();

            if (FilterProductId.HasValue && FilterProductId.Value > 0)
            {
                query = query.Where(r => r.ProductId == FilterProductId.Value);
            }

            if (!string.IsNullOrEmpty(FilterStatus))
            {
                query = query.Where(r => r.Status == FilterStatus);
            }

            var totalCount = await query.CountAsync();
            TotalPages = (int)System.Math.Ceiling(totalCount / (double)PageSize);

            if (PageNumber < 1) PageNumber = 1;
            if (PageNumber > TotalPages) PageNumber = TotalPages == 0 ? 1 : TotalPages;

            var list = await query.OrderByDescending(r => r.CreatedDate)
                                  .Skip((PageNumber - 1) * PageSize)
                                  .Take(PageSize)
                                  .ToListAsync();

            var productIds = list.Select(r => r.ProductId).Distinct().ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id))
                                                 .ToDictionaryAsync(p => p.Id, p => p.Name);

            foreach (var r in list)
            {
                products.TryGetValue(r.ProductId, out var name);
                Reviews.Add(new ReviewView
                {
                    ReviewId = r.ReviewId,
                    ProductId = r.ProductId,
                    ProductName = name ?? "(Unknown)",
                    UserId = r.UserId,
                    UserName = r.UserName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedDate = r.CreatedDate,
                    Status = r.Status
                });
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var r = await _context.Reviews.FindAsync(id);
            if (r == null)
            {
                TempData["error"] = "Không tìm thấy đánh giá.";
                return Redirect(Request.Path + Request.QueryString);
            }
            r.Status = "Approved";
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã duyệt đánh giá.";
            return Redirect(Request.Path + Request.QueryString);
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var r = await _context.Reviews.FindAsync(id);
            if (r == null)
            {
                TempData["error"] = "Không tìm thấy đánh giá.";
                return Redirect(Request.Path + Request.QueryString);
            }
            r.Status = "Rejected";
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã từ chối đánh giá.";
            return Redirect(Request.Path + Request.QueryString);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var r = await _context.Reviews.FindAsync(id);
            if (r == null)
            {
                TempData["error"] = "Không tìm thấy đánh giá.";
                return Redirect(Request.Path + Request.QueryString);
            }
            _context.Reviews.Remove(r);
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã xóa đánh giá.";
            return Redirect(Request.Path + Request.QueryString);
        }
    }
}
