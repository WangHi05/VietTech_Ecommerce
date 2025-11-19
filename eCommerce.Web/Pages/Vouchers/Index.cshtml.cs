using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using eCommerce.Web.Services.Notifications; // Thêm namespace này
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace eCommerce.Web.Pages.Vouchers
{
    public class IndexModel : PageModel // Đổi tên class thành IndexModel cho chuẩn
    {
        private readonly AppDbContext _context;
        private readonly INotificationQueue _queue; // 1. Inject hàng đợi thông báo

        public List<Voucher> AvailableVouchers { get; set; } = new();
        public List<int> UserVoucherIds { get; set; } = new();

        public IndexModel(AppDbContext context, INotificationQueue queue)
        {
            _context = context;
            _queue = queue;
        }

        public async Task OnGetAsync()
        {
            // Logic lấy voucher (Giữ nguyên code của bạn)
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

        // Hàm xử lý khi bấm nút "Lưu voucher"
        public async Task<IActionResult> OnPostCollectAsync(int voucherId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kiểm tra đã lưu chưa
            var exists = await _context.UserVouchers.AnyAsync(uv => uv.UserId == userId && uv.VoucherId == voucherId);
            
            if (!exists)
            {
                // 1. Lưu vào DB
                var userVoucher = new UserVoucher
                {
                    UserId = userId,
                    VoucherId = voucherId,
                    CollectedDate = DateTime.UtcNow, // Dùng UtcNow cho chuẩn
                    IsUsed = false
                };
                _context.UserVouchers.Add(userVoucher);
                await _context.SaveChangesAsync();

                // 2. Lấy thông tin Voucher để gửi thông báo
                var voucher = await _context.Vouchers.FindAsync(voucherId);
                if (voucher != null)
                {
                    // --- LOGIC GỬI PUSH NOTIFICATION ---
                    var discountText = voucher.DiscountPercent.HasValue
                        ? $"{voucher.DiscountPercent}%"
                        : $"{voucher.DiscountAmount:N0}đ";

                    var msg = new NotificationMessage
                    {
                        UserId = userId,
                        Title = "Lưu voucher thành công! ✅",
                        Body = $"Mã {voucher.Code} (Giảm {discountText}) đã vào ví. Dùng ngay!",
                        Url = "/Cart",
                        EnqueuedAt = DateTime.UtcNow
                    };
                    
                    _queue.Enqueue(msg); // Đẩy vào hàng đợi
                }

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