namespace eCommerce.Application.DesignPatterns.SimpleFactory
{
    /// Concrete Product - Standard shipping (5-7 ngày)
    public class StandardShipping : IShippingMethod
    {
        public string GetName() => "Standard Shipping";

        public decimal CalculateCost(decimal orderTotal)
        {
            // Free shipping nếu đơn > 500k
            return orderTotal >= 500000 ? 0 : 30000;
        }

        public int GetEstimatedDays() => 7;
    }
}
