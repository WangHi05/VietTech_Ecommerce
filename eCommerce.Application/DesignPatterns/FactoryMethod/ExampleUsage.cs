namespace eCommerce.Application.DesignPatterns.FactoryMethod
{
    /// Example usage of Factory Method pattern
    public class ExampleUsage
    {
        public static void Demo()
        {
            Console.WriteLine("=== Factory Method Pattern Demo ===\n");

            decimal originalPrice = 500000; // 500k VND

            // Client code sử dụng creator, không biết concrete class
            Console.WriteLine("--- Percentage Discount (20%) ---");
            DiscountCreator percentageCreator = new PercentageDiscountCreator(20);
            percentageCreator.ApplyDiscount(originalPrice);

            Console.WriteLine("\n--- Fixed Amount Discount (100k) ---");
            DiscountCreator fixedCreator = new FixedDiscountCreator(100000);
            fixedCreator.ApplyDiscount(originalPrice);
        }
    }
}
