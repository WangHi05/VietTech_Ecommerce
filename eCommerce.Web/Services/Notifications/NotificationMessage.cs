using System;

namespace eCommerce.Web.Services.Notifications
{
    public class NotificationMessage
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string? EmailTo { get; set; }
        public string? EmailSubject { get; set; }
        public string? EmailBody { get; set; }
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
    }
}
