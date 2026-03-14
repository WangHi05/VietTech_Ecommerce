namespace eCommerce.Application.Decorators.PriceCalculators
{
    public interface IPriceCalculator
    {
        decimal Calculate(decimal basePrice);
    }
}