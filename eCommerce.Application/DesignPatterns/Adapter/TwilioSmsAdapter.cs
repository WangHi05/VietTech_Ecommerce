namespace eCommerce.Application.DesignPatterns.Adapter
{
    /// Adapter - Chuyển đổi interface của Twilio về ISmsService
    public class TwilioSmsAdapter : ISmsService
    {
        private readonly TwilioSmsService _twilioService;
        private readonly string _fromNumber;

        public TwilioSmsAdapter(string accountSid, string authToken, string fromNumber)
        {
            _twilioService = new TwilioSmsService(accountSid, authToken);
            _fromNumber = fromNumber;
        }

        public bool SendSms(string phoneNumber, string message)
        {
            try
            {
                // Adapt: Convert ISmsService.SendSms() -> Twilio.Send()
                var response = _twilioService.Send(
                    to: phoneNumber,
                    body: message,
                    from: _fromNumber
                );

                // Adapt: Convert TwilioResponse -> bool
                return response.Status == "sent" && response.ErrorCode == null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Adapter] Error: {ex.Message}");
                return false;
            }
        }

        public string GetProviderName()
        {
            // Adapt: Convert GetServiceName() -> GetProviderName()
            return _twilioService.GetServiceName();
        }
    }
}
