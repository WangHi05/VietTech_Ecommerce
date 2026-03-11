namespace eCommerce.Application.DesignPatterns.SimpleFactory
{
    /// Concrete Product - Overnight shipping (1 ngày)
    public class OvernightShipping : IShippingMethod
    {
        public string GetName() => "Overnight Shipping";

        public decimal CalculateCost(decimal orderTotal)
        {
            // Phí cao 100k
            return 100000;
        }

        public int GetEstimatedDays() => 1;
    }
}
