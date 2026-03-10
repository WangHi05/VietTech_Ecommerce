using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace eCommerce.Application.Observers
{
    public class CustomerNotificationObserver : IOrderObserver
    {
        // dùng ILogger để in ra console làm ví dụ.
        // có thể Inject IEmailService hoặc IMessageRepository vào đây.
        private readonly ILogger<CustomerNotificationObserver> _logger;

        public CustomerNotificationObserver(ILogger<CustomerNotificationObserver> logger)
        {
            _logger = logger;
        }

        public async Task OrderPaymentStatusChangedAsync(Order order, string paymentStatus)
        {
            if (paymentStatus == "Đã thanh toán")
            {
                // TODO: Gọi hàm gửi Email hoặc lưu tin nhắn vào bảng Messages
                _logger.LogInformation($"[THÔNG BÁO] Hệ thống đã gửi Email: Đơn hàng #{order.Id} của khách hàng {order.UserId} đã thanh toán thành công!");
            }
        }

        public async Task OrderStatusChangedAsync(Order order, string status)
        {
            // Ví dụ: Thông báo khi đơn hàng đang được giao hoặc đã hoàn thành
            // TODO: Gọi hàm Push Notification hoặc tạo Message hệ thống
            _logger.LogInformation($"[THÔNG BÁO] App gửi Push Notification: Đơn hàng #{order.Id} vừa cập nhật trạng thái thành: {status}");
        }
    }
}