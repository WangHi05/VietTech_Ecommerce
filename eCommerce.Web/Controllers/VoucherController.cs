using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using eCommerce.Web.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace eCommerce.Web.Controllers
{
    [Route("api/vouchers")]
    [ApiController]
    [Authorize] // Bắt buộc phải đăng nhập mới được lưu
    public class VoucherController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationQueue _queue;

        public VoucherController(AppDbContext context, INotificationQueue queue)
        {
            _context = context;
            _queue = queue;
        }

        [HttpPost("save/{id}")]
        public async Task<IActionResult> SaveVoucher(int id)
        {
            // 1. Lấy User ID hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 2. Tìm Voucher trong DB
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound(new { message = "Voucher không tồn tại" });

            // 3. Kiểm tra Logic nghiệp vụ (Đã lưu chưa? Hết hạn chưa?)
            // (Giả sử bạn đã có bảng UserVouchers để lưu quan hệ User-Voucher)
            var existing = await _context.UserVouchers
                .FirstOrDefaultAsync(uv => uv.VoucherId == id && uv.UserId == userId);

            if (existing != null)
            {
                return BadRequest(new { message = "Bạn đã lưu voucher này rồi!" });
            }

            // 4. Lưu vào ví người dùng
            var userVoucher = new UserVoucher
            {
                UserId = userId,
                VoucherId = id,
                CollectedDate = DateTime.UtcNow,
                IsUsed = false
            };
            
            // Giả định bạn đã có DbSet UserVouchers. Nếu chưa, hãy thêm vào DbContext
            _context.UserVouchers.Add(userVoucher); 
            await _context.SaveChangesAsync();

            // ============================================================
            // 5. LOGIC QUAN TRỌNG: Gửi thông báo cá nhân hóa (Kịch bản 2)
            // ============================================================
            
            var discountText = voucher.DiscountPercent.HasValue 
                ? $"{voucher.DiscountPercent}%" 
                : $"{voucher.DiscountAmount:N0}đ";

            var msg = new NotificationMessage
            {
                UserId = userId, // CHỈ GỬI CHO NGƯỜI BẤM LƯU
                Title = "Đã lưu voucher thành công! ✅",
                Body = $"Mã {voucher.Code} (Giảm {discountText}) đã vào ví của bạn. Dùng ngay kẻo hết!",
                Url = "/Cart", // Trỏ về giỏ hàng để thúc đẩy mua sắm
                EnqueuedAt = DateTime.UtcNow
            };

            _queue.Enqueue(msg); // Đẩy vào hàng đợi

            return Ok(new { success = true, message = "Lưu thành công!" });
        }
    }
}