namespace eCommerce.Application.DesignPatterns.SimpleFactory
{
    /// Concrete Product - Express shipping (2-3 ngày)
    public class ExpressShipping : IShippingMethod
    {
        public string GetName() => "Express Shipping";

        public decimal CalculateCost(decimal orderTotal)
        {
            // Phí cố định 50k
            return 50000;
        }

        public int GetEstimatedDays() => 3;
    }
}
