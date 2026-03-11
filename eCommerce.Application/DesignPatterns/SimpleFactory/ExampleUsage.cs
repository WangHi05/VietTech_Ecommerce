namespace eCommerce.Application.DesignPatterns.SimpleFactory
{
    /// Example usage of Simple Factory pattern
    public class ExampleUsage
    {
        public static void Demo()
        {
            Console.WriteLine("=== Simple Factory Pattern Demo ===\n");

            decimal orderTotal = 600000; // 600k VND

            // Client chỉ cần biết type, không cần biết cách tạo object
            var standard = ShippingFactory.CreateShippingMethod("standard");
            Console.WriteLine($"{standard.GetName()}:");
            Console.WriteLine($"  Cost: {standard.CalculateCost(orderTotal):N0} VND");
            Console.WriteLine($"  Days: {standard.GetEstimatedDays()} days\n");

            var express = ShippingFactory.CreateShippingMethod("express");
            Console.WriteLine($"{express.GetName()}:");
            Console.WriteLine($"  Cost: {express.CalculateCost(orderTotal):N0} VND");
            Console.WriteLine($"  Days: {express.GetEstimatedDays()} days\n");

            var overnight = ShippingFactory.CreateShippingMethod("overnight");
            Console.WriteLine($"{overnight.GetName()}:");
            Console.WriteLine($"  Cost: {overnight.CalculateCost(orderTotal):N0} VND");
            Console.WriteLine($"  Days: {overnight.GetEstimatedDays()} days");
        }
    }
}
