namespace eCommerce.Application.DesignPatterns.Adapter
{
    /// Adaptee - Third-party Twilio service với interface khác
    /// (Giả lập external API)
    public class TwilioSmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;

        public TwilioSmsService(string accountSid, string authToken)
        {
            _accountSid = accountSid;
            _authToken = authToken;
        }

        // Twilio sử dụng method name khác
        public TwilioResponse Send(string to, string body, string from)
        {
            // Giả lập gọi API Twilio
            Console.WriteLine($"[Twilio API] Sending SMS to {to}");
            Console.WriteLine($"[Twilio API] From: {from}");
            Console.WriteLine($"[Twilio API] Body: {body}");

            return new TwilioResponse
            {
                Status = "sent",
                MessageSid = $"SM{Guid.NewGuid():N}",
                ErrorCode = null
            };
        }

        public string GetServiceName() => "Twilio SMS Gateway";
    }
    /// Twilio response object
    public class TwilioResponse
    {
        public string Status { get; set; } = string.Empty;
        public string MessageSid { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
    }
}
