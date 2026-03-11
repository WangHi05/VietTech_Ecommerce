namespace eCommerce.Application.DesignPatterns.Adapter
{
    /// Example usage of Adapter pattern
    public class ExampleUsage
    {
        public static void Demo()
        {
            Console.WriteLine("=== Adapter Pattern Demo ===\n");

            // Client code chỉ biết ISmsService interface
            ISmsService smsService = new TwilioSmsAdapter(
                accountSid: "AC1234567890",
                authToken: "your_auth_token",
                fromNumber: "+84901234567"
            );

            Console.WriteLine($"Provider: {smsService.GetProviderName()}\n");

            // Client gọi method theo ISmsService interface
            bool success = smsService.SendSms(
                phoneNumber: "+84912345678",
                message: "Your order has been shipped!"
            );

            Console.WriteLine($"\nSMS sent: {(success ? "Success" : "Failed")}");
        }
    }
}
