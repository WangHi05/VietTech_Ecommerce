namespace eCommerce.Web.Services.Notifications
{
    /// <summary>
    /// TEMPLATE METHOD PATTERN — Class Con (Web Push)
    /// Kế thừa NotificationSenderTemplateMethod, chỉ implement cách build JSON payload và gửi qua WebPush.
    /// </summary>
    public class PushNotificationSender : NotificationSenderTemplateMethod
    {
        private readonly IPushService _pushService;

        public PushNotificationSender(IPushService pushService)
        {
            _pushService = pushService;
        }

        // Override bước 2: format thành plain text gọn (Push không cần HTML)
        protected override string BuildMessage(string subject, string body)
        {
            // Trả về body đơn giản; title dùng riêng trong SendCoreAsync
            return body;
        }

        // Override bước 3: gửi qua WebPushService theo userId
        protected override async Task SendCoreAsync(string userId, string recipientAddress, string subject, string message)
        {
            // recipientAddress ở kênh Push chính là userId (không dùng email)
            await _pushService.SendPushAsync(userId, subject, message, new { url = "/" });
        }

        // Override hook: ghi log ra console
        protected override Task OnSentAsync(string userId, string recipientAddress, string subject)
        {
            Console.WriteLine($"[PushNotificationSender] Đã gửi push notification tới userId={userId} | Tiêu đề: {subject}");
            return Task.CompletedTask;
        }
    }
}
