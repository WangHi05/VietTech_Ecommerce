namespace eCommerce.Application.DesignPatterns.FactoryMethod
{
    /// Concrete Creator - Tạo fixed amount discount
    public class FixedDiscountCreator : DiscountCreator
    {
        private readonly decimal _amount;

        public FixedDiscountCreator(decimal amount)
        {
            _amount = amount;
        }

        // Factory Method implementation
        public override IDiscountCalculator CreateDiscount()
        {
            return new FixedAmountDiscount(_amount);
        }
    }
}
