namespace eCommerce.Application.DesignPatterns.FactoryMethod
{
    /// Concrete Creator - Tạo percentage discount
    public class PercentageDiscountCreator : DiscountCreator
    {
        private readonly decimal _percentage;

        public PercentageDiscountCreator(decimal percentage)
        {
            _percentage = percentage;
        }

        // Factory Method implementation
        public override IDiscountCalculator CreateDiscount()
        {
            return new PercentageDiscount(_percentage);
        }
    }
}
