using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Web.Areas.Admin.Pages.Stock
{
    [Authorize(Roles = "Admin")]
    public class HistoryModel : PageModel
    {
        private readonly AppDbContext _context;

        public HistoryModel(AppDbContext context)
        {
            _context = context;
        }

        public List<StockHistory> StockHistories { get; set; } = new();
        public List<Product> Products { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? ProductId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Type { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Limit { get; set; } = 100;

        public async Task<IActionResult> OnGetAsync()
        {
            // Load products for filter dropdown
            Products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            // Build query
            var query = _context.StockHistories
                .Include(h => h.Product)
                .AsQueryable();

            // Apply filters
            if (ProductId.HasValue)
            {
                query = query.Where(h => h.ProductId == ProductId.Value);
            }

            if (!string.IsNullOrEmpty(Type))
            {
                query = query.Where(h => h.Type == Type);
            }

            // Get results
            StockHistories = await query
                .OrderByDescending(h => h.CreatedAt)
                .Take(Limit)
                .ToListAsync();

            return Page();
        }
    }
}
