using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using eCommerce.Web.Services.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.Web.Areas.Admin.Pages.Vouchers
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly INotificationQueue _queue;

        public CreateModel(AppDbContext context, INotificationQueue queue)
        {
            _context = context;
            _queue = queue;
        }

        [BindProperty]
        public Voucher Voucher { get; set; } = new Voucher();

        [BindProperty]
        [Display(Name = "Gửi thông báo (Push Notification) cho tất cả khách hàng?")]
        public bool SendPushNotification { get; set; } = true;

        public IActionResult OnGet()
        {
            // Khởi tạo giá trị mặc định
            Voucher.StartDate = DateTime.Today;
            Voucher.ExpiryDate = DateTime.Today.AddDays(7); // Mặc định 7 ngày
            Voucher.IsActive = true;
            Voucher.MaxUsage = 100; // Mặc định số lượng
            Voucher.MaxUsagePerUser = 1;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Loại bỏ ModelState check cho các thuộc tính không cần thiết nếu cần
            // Nhưng với Entity của bạn, các rule [Required] đã chuẩn rồi.
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. Kiểm tra trùng mã Code
            if (await _context.Vouchers.AnyAsync(v => v.Code == Voucher.Code))
            {
                ModelState.AddModelError("Voucher.Code", "Mã voucher này đã tồn tại.");
                return Page();
            }

            // Xử lý logic: Nếu nhập cả % và Tiền mặt thì ưu tiên cái nào? 
            // Hoặc validation không cho nhập cả 2 (tùy bạn).
            // Ở đây tôi để mặc định là lưu cả 2 vào DB.

            _context.Vouchers.Add(Voucher);
            await _context.SaveChangesAsync();

            // 2. Gửi thông báo hàng loạt
            if (SendPushNotification)
            {
                var subscribedUserIds = await _context.UserPushSubscriptions
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToListAsync();

                if (subscribedUserIds.Any())
                {
                    foreach (var userId in subscribedUserIds)
                    {
                        // Tạo nội dung thông báo dựa trên Entity Voucher
                        var discountText = Voucher.DiscountPercent.HasValue 
                                            ? $"{Voucher.DiscountPercent}%" 
                                            : $"{Voucher.DiscountAmount:N0}đ";

                        var msg = new NotificationMessage
                        {
                            UserId = userId,
                            Title = $"🎁 Mã mới: {Voucher.Code}", 
                            Body = $"{Voucher.Description}. Giảm {discountText}. HSD: {Voucher.ExpiryDate:dd/MM}.",
                            Url = "/Vouchers", 
                            EnqueuedAt = DateTime.UtcNow
                        };

                        _queue.Enqueue(msg);
                    }
                }
            }

            TempData["success"] = "Tạo voucher thành công!";
            return RedirectToPage("./Index");
        }
    }
}