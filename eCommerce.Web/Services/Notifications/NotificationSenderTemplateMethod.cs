namespace eCommerce.Web.Services.Notifications
{
    // TEMPLATE METHOD PATTERN: Bộ khung gửi thông báo
    public abstract class NotificationSenderTemplateMethod
    {
        // Template Method: Định nghĩa các bước xử lý cố định
        public async Task SendAsync(string userId, string recipientAddress, string subject, string body)
        {
            if (!ValidateRecipient(recipientAddress))
            {
                Console.WriteLine($"[Notification] Invalid recipient: {recipientAddress}");
                return;
            }

            var message = BuildMessage(subject, body);
            await SendCoreAsync(userId, recipientAddress, subject, message);
            await OnSentAsync(userId, recipientAddress, subject);
        }

        protected bool ValidateRecipient(string recipientAddress)
        {
            return !string.IsNullOrWhiteSpace(recipientAddress);
        }

        // Abstract methods: Subclass bắt buộc implement
        protected abstract string BuildMessage(string subject, string body);
        protected abstract Task SendCoreAsync(string userId, string recipientAddress, string subject, string message);

        // Hook: Subclass có thể override
        protected virtual Task OnSentAsync(string userId, string recipientAddress, string subject)
        {
            return Task.CompletedTask;
        }
    }
}
