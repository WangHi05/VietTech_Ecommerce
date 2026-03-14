namespace eCommerce.Application.Decorators.PriceCalculators
{
    public class LoyaltyDecorator : IPriceCalculator
    {
        private readonly IPriceCalculator _inner;
        private readonly decimal _pointsDiscount;

        public LoyaltyDecorator(IPriceCalculator inner, decimal pointsDiscount)
        {
            _inner = inner;
            _pointsDiscount = pointsDiscount;
        }

        public decimal Calculate(decimal basePrice)
        {
            return _inner.Calculate(basePrice) - _pointsDiscount;
        }
    }
}