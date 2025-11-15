using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using eCommerce.Application.Services;
using eCommerce.Web.Services;
using eCommerce.Web.Services.Notifications;
using System.Threading.Tasks;

namespace eCommerce.Web.Areas.Admin.Pages.Orders
{
    public class DetailsModel : PageModel
    {
    private readonly IOrderRepository _orderRepo;
    private readonly IOrderService _orderService;
    private readonly IPushService _pushService;
    private readonly Microsoft.AspNetCore.Identity.UI.Services.IEmailSender _emailSender;
    private readonly eCommerce.Web.Services.Notifications.INotificationQueue _notificationQueue;

        public DetailsModel(IOrderRepository orderRepo, IOrderService orderService, IPushService pushService, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender, eCommerce.Web.Services.Notifications.INotificationQueue notificationQueue)
        {
            _orderRepo = orderRepo;
            _orderService = orderService;
            _pushService = pushService;
            _emailSender = emailSender;
            _notificationQueue = notificationQueue;
        }

        // Thùng chứa cho 1 đơn hàng
    public Order? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Dùng hàm chi tiết mà chúng ta đã tạo
            Order = await _orderRepo.GetDetailsByIdForAdminAsync(id);

            if (Order == null)
            {
                // Nếu không tìm thấy đơn hàng, quay về danh sách
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage("./Index");
            }

            return Page();
        }

        // Admin xác nhận đơn (chuyển trạng thái sang "Đang Giao")
        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            await _orderService.UpdateStatusAsync(id, "Đang Giao");

            // Load order to get user/email
            var order = await _orderRepo.GetDetailsByIdForAdminAsync(id);
            if (order != null)
            {
                // enqueue notification + email
                var msg = new eCommerce.Web.Services.Notifications.NotificationMessage
                {
                    UserId = order.UserId ?? string.Empty,
                    Title = "Đơn hàng đang giao",
                    Body = $"Đơn #{order.Id} đang được giao.",
                    Url = $"/Orders/Details?id={order.Id}",
                    EmailTo = order.Customer?.Email,
                    EmailSubject = $"Đơn hàng #{order.Id} - Đang Giao",
                    EmailBody = $"Xin chào {order.ShippingName},<br/><br/>Đơn hàng #{order.Id} của bạn đã được xác nhận và đang trên đường giao.<br/><br/>Cảm ơn bạn."
                };

                try
                {
                    _notificationQueue.Enqueue(msg);
                }
                catch { }
            }

            TempData["success"] = "Đã xác nhận đơn hàng.";
            return RedirectToPage("./Details", new { id });
        }

        // Admin huỷ đơn (chuyển trạng thái sang "Không được xác nhận")
        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            await _orderService.UpdateStatusAsync(id, "Không được xác nhận");

            var order = await _orderRepo.GetDetailsByIdForAdminAsync(id);
            if (order != null)
            {
                var msg = new eCommerce.Web.Services.Notifications.NotificationMessage
                {
                    UserId = order.UserId ?? string.Empty,
                    Title = "Đơn hàng bị huỷ",
                    Body = $"Đơn #{order.Id} của bạn không được xác nhận.",
                    Url = $"/Orders/Details?id={order.Id}",
                    EmailTo = order.Customer?.Email,
                    EmailSubject = $"Đơn hàng #{order.Id} - Không được xác nhận",
                    EmailBody = $"Xin chào {order.ShippingName},<br/><br/>Rất tiếc, đơn hàng #{order.Id} của bạn đã bị huỷ. Vui lòng liên hệ support nếu cần hỗ trợ.<br/><br/>Cảm ơn."
                };

                try { _notificationQueue.Enqueue(msg); } catch { }
            }

            TempData["success"] = "Đơn hàng đã bị huỷ.";
            return RedirectToPage("./Details", new { id });
        }

        // Admin đánh dấu đơn hàng hoàn thành
        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            await _orderService.UpdateStatusAsync(id, "Hoàn thành");

            var order = await _orderRepo.GetDetailsByIdForAdminAsync(id);
            if (order != null)
            {
                var msg = new eCommerce.Web.Services.Notifications.NotificationMessage
                {
                    UserId = order.UserId ?? string.Empty,
                    Title = "Đơn hàng hoàn thành",
                    Body = $"Đơn #{order.Id} đã được giao thành công!",
                    Url = $"/Orders/Details?id={order.Id}",
                    EmailTo = order.Customer?.Email,
                    EmailSubject = $"Đơn hàng #{order.Id} - Hoàn Thành",
                    EmailBody = $"Xin chào {order.ShippingName},<br/><br/>Đơn hàng #{order.Id} đã được giao thành công. Cảm ơn bạn đã mua hàng!<br/><br/>Bạn đã nhận được điểm thưởng cho đơn hàng này."
                };

                try { _notificationQueue.Enqueue(msg); } catch { }
            }

            TempData["success"] = "Đã đánh dấu đơn hàng hoàn thành và tích điểm cho khách hàng.";
            return RedirectToPage("./Details", new { id });
        }
    }
}