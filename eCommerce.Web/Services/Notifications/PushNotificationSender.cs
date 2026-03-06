namespace eCommerce.Web.Services.Notifications
{
    public class PushNotificationSender : NotificationSenderTemplateMethod
    {
        private readonly IPushService _pushService;

        public PushNotificationSender(IPushService pushService)
        {
            _pushService = pushService;
        }

        protected override string BuildMessage(string subject, string body)
        {
            return body;
        }

        protected override async Task SendCoreAsync(string userId, string recipientAddress, string subject, string message)
        {
            await _pushService.SendPushAsync(userId, subject, message, new { url = "/" });
        }

        protected override Task OnSentAsync(string userId, string recipientAddress, string subject)
        {
            Console.WriteLine($"[PushNotificationSender] Đã gửi push notification tới userId={userId} | Tiêu đề: {subject}");
            return Task.CompletedTask;
        }
    }
}
