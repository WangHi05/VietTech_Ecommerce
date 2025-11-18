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
    public class ImportModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IStockService _stockService;

        public ImportModel(AppDbContext context, IStockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
        public int ProductId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập lý do")]
        [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new();
        public List<Product> LowStockProducts { get; set; } = new();

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
                var userName = User.Identity?.Name ?? "Admin";
                await _stockService.ImportStock(ProductId, Quantity, Reason, userName);

                var product = await _context.Products.FindAsync(ProductId);
                TempData["SuccessMessage"] = $"Đã nhập {Quantity} sản phẩm '{product?.Name}' vào kho thành công!";
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi nhập hàng: {ex.Message}";
                await LoadDataAsync();
                return Page();
            }
        }

        private async Task LoadDataAsync()
        {
            Products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            LowStockProducts = await _stockService.GetLowStockProducts();
        }
    }
}
