namespace eCommerce.Application.DesignPatterns.Adapter
{
    /// Target interface - Interface mà client mong đợi
    public interface ISmsService
    {
        bool SendSms(string phoneNumber, string message);
        string GetProviderName();
    }
}
