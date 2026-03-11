namespace eCommerce.Application.DesignPatterns.SimpleFactory
{
    /// Simple Factory - Tạo shipping method dựa vào type
    public class ShippingFactory
    {
        public static IShippingMethod CreateShippingMethod(string shippingType)
        {
            return shippingType.ToLower() switch
            {
                "standard" => new StandardShipping(),
                "express" => new ExpressShipping(),
                "overnight" => new OvernightShipping(),
                _ => throw new ArgumentException($"Unknown shipping type: {shippingType}")
            };
        }
    }
}
