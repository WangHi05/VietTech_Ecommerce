using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using eCommerce.Core.Entities;
using eCommerce.Web.Services.Notifications;

namespace eCommerce.Web.Controllers
{
    [ApiController]
    [Route("api/push")]
    public class PushController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PushController(AppDbContext db)
        {
            _db = db;
        }

/*
    [HttpGet("force-test")]
    public IActionResult ForceTest([FromServices] INotificationQueue queue)
    {
        // 1. Lấy ID người dùng đang đăng nhập
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Content("Lỗi: Bạn chưa đăng nhập! Hãy đăng nhập trước.");

        // 2. Tạo thông báo giả
        var msg = new NotificationMessage
        {
            UserId = userId, // Gửi cho chính mình
            Title = "🔔 Test thành công!",
            Body = $"Thông báo lúc {DateTime.Now:HH:mm:ss}. Click để xem giỏ hàng.",
            Url = "/Cart"
        };

        // 3. Đẩy vào hàng đợi (Background Service sẽ lo phần còn lại)
        queue.Enqueue(msg);

        return Content($"Đã gửi lệnh push cho User ID: {userId}. Hãy kiểm tra thông báo!");
    }
*/
    public class SubscribeRequest
    {
            public string? endpoint { get; set; }
            public Keys? keys { get; set; }
        }

        public class Keys
        {
            public string? p256dh { get; set; }
            public string? auth { get; set; }
        }

        [HttpPost("subscribe")]
        [Authorize]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest req)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (req == null || string.IsNullOrEmpty(req.endpoint) || req.keys == null) return BadRequest();

            var exists = await _db.UserPushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == req.endpoint && s.UserId == userId);
            if (exists == null)
            {
                var s = new UserPushSubscription
                {
                    UserId = userId,
                    Endpoint = req.endpoint,
                    P256dh = req.keys.p256dh ?? string.Empty,
                    Auth = req.keys.auth ?? string.Empty
                };
                _db.UserPushSubscriptions.Add(s);
                await _db.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
