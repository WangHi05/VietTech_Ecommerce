using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Web.Areas.Admin.Pages.Messages
{
    [Authorize(Roles = "Admin")]
    public class ChatModel : PageModel
    {
        private readonly AppDbContext _context;

        public ChatModel(AppDbContext context)
        {
            _context = context;
        }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            ProductId = productId;
            ProductName = product.Name;
            return Page();
        }
    }
}
