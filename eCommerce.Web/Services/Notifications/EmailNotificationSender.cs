using Microsoft.AspNetCore.Identity.UI.Services;

namespace eCommerce.Web.Services.Notifications
{
    public class EmailNotificationSender : NotificationSenderTemplateMethod
    {
        private readonly IEmailSender _emailSender;

        public EmailNotificationSender(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        protected override string BuildMessage(string subject, string body)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                    <h2 style='color: #e44d26;'>VietTech Store</h2>
                    <h3>{subject}</h3>
                    <p>{body}</p>
                    <hr/>
                    <small style='color: gray;'>Email tự động từ hệ thống VietTech Store.</small>
                </div>";
        }

        protected override async Task SendCoreAsync(string userId, string recipientAddress, string subject, string message)
        {
            await _emailSender.SendEmailAsync(recipientAddress, subject, message);
        }

        protected override Task OnSentAsync(string userId, string recipientAddress, string subject)
        {
            Console.WriteLine($"[EmailNotificationSender] Đã gửi email tới {recipientAddress} | Tiêu đề: {subject}");
            return Task.CompletedTask;
        }
    }
}
