using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data; 
using eCommerce.Web.Services.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.Web.Areas.Admin.Pages.Notifications
{
    public class SendModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly INotificationQueue _queue;

        public SendModel(AppDbContext context, INotificationQueue queue)
        {
            _context = context;
            _queue = queue;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
            [Display(Name = "Tiêu đề thông báo")]
            public string Title { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập nội dung")]
            [Display(Name = "Nội dung ngắn")]
            public string Body { get; set; }

            [Display(Name = "Đường dẫn khi click (VD: /Products/Sale)")]
            public string Url { get; set; } = "/";
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // 1. Lấy danh sách tất cả User ID đã đăng ký nhận thông báo
            // Dùng Distinct() để tránh gửi trùng nếu 1 user đăng ký trên nhiều thiết bị
            var subscribedUserIds = await _context.UserPushSubscriptions
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();

            if (!subscribedUserIds.Any())
            {
                TempData["error"] = "Chưa có người dùng nào đăng ký nhận thông báo.";
                return Page();
            }

            // 2. Tạo vòng lặp gửi tin nhắn vào hàng đợi (Queue)
            int count = 0;
            foreach (var userId in subscribedUserIds)
            {
                var msg = new NotificationMessage
                {
                    UserId = userId,
                    Title = Input.Title, // VD: "Siêu Sale 11/11 🔥"
                    Body = Input.Body,   // VD: "Giảm giá 50% toàn bộ giày Nike..."
                    Url = Input.Url,     // VD: "/Products/Details?id=5" hoặc "/Category/Sale"
                    EnqueuedAt = DateTime.UtcNow
                };

                _queue.Enqueue(msg);
                count++;
            }

            // 

            TempData["success"] = $"Đã đẩy {count} thông báo vào hàng đợi xử lý.";
            return RedirectToPage(); // Load lại trang để clear form
        }
    }
}