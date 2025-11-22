using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Web.Areas.Admin.Pages.Messages
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<ProductChatSummary> ProductChats { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Get all products that have messages
            ProductChats = await _context.Messages
                .GroupBy(m => m.ProductId)
                .Select(g => new ProductChatSummary
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product!.Name,
                    LastMessage = g.OrderByDescending(m => m.CreatedAt).First().Content,
                    LastMessageTime = g.Max(m => m.CreatedAt),
                    UnreadCount = g.Count(m => !m.IsFromSeller) // Count customer messages
                })
                .OrderByDescending(p => p.LastMessageTime)
                .ToListAsync();
        }

        public class ProductChatSummary
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string LastMessage { get; set; } = string.Empty;
            public DateTime LastMessageTime { get; set; }
            public int UnreadCount { get; set; }
        }
    }
}
