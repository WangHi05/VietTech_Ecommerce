using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.Web.Areas.Admin.Pages.Stock
{
    [Authorize(Roles = "Admin")]
    public class AdjustModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IStockService _stockService;

        public AdjustModel(AppDbContext context, IStockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
        public int ProductId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập số lượng mới")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int NewQuantity { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập lý do")]
        [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            try
            {
                var product = await _context.Products.FindAsync(ProductId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm!";
                    await LoadDataAsync();
                    return Page();
                }

                var oldQuantity = product.StockQuantity;
                var userName = User.Identity?.Name ?? "Admin";
                
                await _stockService.AdjustStock(ProductId, NewQuantity, Reason, userName);

                var difference = NewQuantity - oldQuantity;
                var changeText = difference > 0 ? $"tăng {difference}" : $"giảm {Math.Abs(difference)}";
                
                TempData["SuccessMessage"] = $"Đã điều chỉnh tồn kho '{product.Name}' từ {oldQuantity} → {NewQuantity} ({changeText})";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi điều chỉnh: {ex.Message}";
                await LoadDataAsync();
                return Page();
            }
        }

        private async Task LoadDataAsync()
        {
            Products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}
