namespace eCommerce.Application.DesignPatterns.FactoryMethod
{
    /// Concrete Product - Percentage discount (10%, 20%, 50%...)
    public class PercentageDiscount : IDiscountCalculator
    {
        private readonly decimal _percentage;

        public PercentageDiscount(decimal percentage)
        {
            _percentage = percentage;
        }

        public decimal Calculate(decimal originalPrice)
        {
            return originalPrice * (1 - _percentage / 100);
        }

        public string GetDescription()
        {
            return $"{_percentage}% off";
        }
    }
}
