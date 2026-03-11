namespace eCommerce.Application.DesignPatterns.FactoryMethod
{
    /// Product interface - Discount calculator
    public interface IDiscountCalculator
    {
        decimal Calculate(decimal originalPrice);
        string GetDescription();
    }
}
