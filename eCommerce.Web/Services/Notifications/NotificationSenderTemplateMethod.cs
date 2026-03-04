namespace eCommerce.Web.Services.Notifications
{
    /// <summary>
    /// TEMPLATE METHOD PATTERN
    /// Abstract base class định nghĩa khung gửi thông báo — các bước cố định,
    /// chỉ cách build message và cách send là khác nhau giữa các kênh (Email / Push).
    /// </summary>
    public abstract class NotificationSenderTemplateMethod
    {
        // ============================================================
        // TEMPLATE METHOD: Bộ khung cố định, không cho override
        // ============================================================
        public async Task SendAsync(string userId, string recipientAddress, string subject, string body)
        {
            // Bước 1: Validate người nhận (chung — không override)
            if (!ValidateRecipient(recipientAddress))
            {
                Console.WriteLine($"[Notification] Invalid recipient: {recipientAddress}");
                return;
            }

            // Bước 2: Build nội dung thông báo (KHÁC NHAU → abstract, subclass tự làm)
            var message = BuildMessage(subject, body);

            // Bước 3: Gửi thông báo (KHÁC NHAU → abstract, subclass tự làm)
            await SendCoreAsync(userId, recipientAddress, subject, message);

            // Bước 4: Hook — log sau khi gửi (subclass CÓ THỂ override)
            await OnSentAsync(userId, recipientAddress, subject);
        }

        // ============================================================
        // Bước chung (không abstract) — dùng chung cho tất cả kênh
        // ============================================================
        protected bool ValidateRecipient(string recipientAddress)
        {
            return !string.IsNullOrWhiteSpace(recipientAddress);
        }

        // ============================================================
        // Abstract steps — subclass BẮT BUỘC implement
        // ============================================================

        /// <summary>Mỗi kênh format message theo cách riêng (HTML, JSON, plain text...)</summary>
        protected abstract string BuildMessage(string subject, string body);

        /// <summary>Mỗi kênh gửi theo cơ chế riêng (SMTP, WebPush...)</summary>
        protected abstract Task SendCoreAsync(string userId, string recipientAddress, string subject, string message);

        // ============================================================
        // Hook — subclass CÓ THỂ override để thêm log / audit
        // ============================================================
        protected virtual Task OnSentAsync(string userId, string recipientAddress, string subject)
        {
            return Task.CompletedTask;
        }
    }
}
