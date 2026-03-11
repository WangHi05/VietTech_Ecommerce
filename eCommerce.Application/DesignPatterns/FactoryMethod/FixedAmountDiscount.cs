namespace eCommerce.Application.DesignPatterns.FactoryMethod
{
    /// Concrete Product - Fixed amount discount (50k, 100k, 200k...)
    public class FixedAmountDiscount : IDiscountCalculator
    {
        private readonly decimal _amount;

        public FixedAmountDiscount(decimal amount)
        {
            _amount = amount;
        }

        public decimal Calculate(decimal originalPrice)
        {
            var finalPrice = originalPrice - _amount;
            return finalPrice > 0 ? finalPrice : 0;
        }

        public string GetDescription()
        {
            return $"{_amount:N0} VND off";
        }
    }
}
