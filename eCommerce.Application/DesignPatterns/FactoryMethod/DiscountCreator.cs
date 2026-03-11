namespace eCommerce.Application.DesignPatterns.FactoryMethod
{
    /// Creator (abstract) - Định nghĩa Factory Method
    public abstract class DiscountCreator
    {
        // Factory Method - subclass sẽ override
        public abstract IDiscountCalculator CreateDiscount();

        // Template method sử dụng Factory Method
        public void ApplyDiscount(decimal originalPrice)
        {
            var calculator = CreateDiscount();
            var finalPrice = calculator.Calculate(originalPrice);

            Console.WriteLine($"Original Price: {originalPrice:N0} VND");
            Console.WriteLine($"Discount: {calculator.GetDescription()}");
            Console.WriteLine($"Final Price: {finalPrice:N0} VND");
        }
    }
}
