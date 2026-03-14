namespace eCommerce.Application.Decorators.PriceCalculators
{
    public class BasePriceCalculator : IPriceCalculator
    {
        public decimal Calculate(decimal basePrice) => basePrice;
    }
}