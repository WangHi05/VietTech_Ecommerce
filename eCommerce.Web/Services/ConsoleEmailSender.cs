using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace eCommerce.Web.Services
{
    // Implementation đơn giản của IEmailSender chỉ ghi ra Console
    // Trong môi trường production, bạn cần thay thế bằng dịch vụ gửi email thật (SendGrid, Mailgun, SMTP...)
    public class ConsoleEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine("--- NEW EMAIL ---");
            Console.WriteLine($"To: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine("Body (HTML):");
            Console.WriteLine(htmlMessage);
            Console.WriteLine("--- END EMAIL ---");
            return Task.CompletedTask; // Giả lập gửi thành công
        }
    }
}