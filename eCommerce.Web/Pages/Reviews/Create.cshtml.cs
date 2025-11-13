using System;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eCommerce.Web.Pages.Reviews
{
    [Authorize]
    public class CreateModel : PageModel
    {
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public string[] ProhibitedWords { get; set; } = Array.Empty<string>();

        public CreateModel(AppDbContext context, UserManager<ApplicationUser> userManager, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public class InputModel
        {
            [Required]
            [Range(1,5, ErrorMessage = "Rating must be between 1 and 5")]
            public int Rating { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập bình luận")]
            [StringLength(2000, ErrorMessage = "Bình luận quá dài")]
            public string Comment { get; set; } = string.Empty;

            [Required]
            public int ProductId { get; set; }

            [Required]
            public int OrderId { get; set; }
        }

        [BindProperty]
        public InputModel Review { get; set; } = new InputModel();

    // optional product display info
    public string? ProductName { get; set; }

        // Fill hidden fields when navigated from Orders/Details
        public void OnGet(int productId, int orderId)
        {
            Review.ProductId = productId;
            Review.OrderId = orderId;

            // load product name for display (non-blocking simple lookup)
            try
            {
                var p = _context.Products.Find(productId);
                ProductName = p?.Name;
            }
            catch
            {
                // ignore DB lookup failure on GET - name is optional
            }
            // load prohibited words from configuration for client-side script
            try
            {
                var list = _configuration.GetSection("ReviewModeration:ProhibitedWords").Get<string[]>();
                if (list != null && list.Length > 0)
                {
                    ProhibitedWords = list;
                }
            }
            catch
            {
                ProhibitedWords = Array.Empty<string>();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Challenge();

            var user = await _userManager.FindByIdAsync(userId);
            var userName = user?.UserName ?? user?.FullName ?? string.Empty;

            // load prohibited words from configuration (server-side)
            var prohibited = _configuration.GetSection("ReviewModeration:ProhibitedWords").Get<string[]>() ?? new[] { "spamword1", "spamword2", "violation", "xxx" };
            var commentLower = (Review.Comment ?? string.Empty).ToLowerInvariant();
            foreach (var bad in prohibited)
            {
                if (string.IsNullOrWhiteSpace(bad)) continue;
                // match whole words to reduce false positives
                var pattern = $"\\b{Regex.Escape(bad.ToLowerInvariant())}\\b";
                if (Regex.IsMatch(commentLower, pattern, RegexOptions.CultureInvariant))
                {
                    ModelState.AddModelError("Review.Comment", "Bình luận chứa nội dung không hợp lệ và không được phép.");
                    return Page();
                }
            }

            var entity = new Review
            {
                ProductId = Review.ProductId,
                UserId = userId,
                UserName = userName,
                Rating = Review.Rating,
                Comment = Review.Comment ?? string.Empty,
                CreatedDate = DateTime.UtcNow,
                // auto-approve so user sees review immediately
                Status = "Approved",
                OrderId = Review.OrderId
            };

            _context.Reviews.Add(entity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã đánh giá";

            return RedirectToPage("/Orders/Details", new { id = Review.OrderId });
        }
    }
}
