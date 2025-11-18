using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace eCommerce.Web.Pages
{
    public class VouchersModel : PageModel
    {
        private readonly AppDbContext _context;
        public List<Voucher> AvailableVouchers { get; set; } = new();
        public List<int> UserVoucherIds { get; set; } = new();

        public VouchersModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            AvailableVouchers = await _context.Vouchers
                .Where(v => v.IsActive && v.ExpiryDate > DateTime.Now && v.StartDate <= DateTime.Now)
                .OrderByDescending(v => v.ExpiryDate)
                .ToListAsync();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                UserVoucherIds = await _context.UserVouchers
                    .Where(uv => uv.UserId == userId)
                    .Select(uv => uv.VoucherId)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostCollectAsync(int voucherId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                TempData["Error"] = "Vui lòng đăng nhập để lưu voucher.";
                return RedirectToPage();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var exists = await _context.UserVouchers.AnyAsync(uv => uv.UserId == userId && uv.VoucherId == voucherId);
            if (!exists)
            {
                _context.UserVouchers.Add(new UserVoucher
                {
                    UserId = userId,
                    VoucherId = voucherId,
                    CollectedDate = DateTime.Now,
                    IsUsed = false
                });
                await _context.SaveChangesAsync();
                TempData["Message"] = "Đã lưu voucher thành công!";
            }
            else
            {
                TempData["Error"] = "Bạn đã lưu voucher này rồi.";
            }
            return RedirectToPage();
        }
    }
}
