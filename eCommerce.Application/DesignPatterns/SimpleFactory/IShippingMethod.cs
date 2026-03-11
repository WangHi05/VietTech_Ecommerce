namespace eCommerce.Application.DesignPatterns.SimpleFactory
{
    /// Product interface - Định nghĩa shipping method
    public interface IShippingMethod
    {
        string GetName();
        decimal CalculateCost(decimal orderTotal);
        int GetEstimatedDays();
    }
}
