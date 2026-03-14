namespace eCommerce.Application.Decorators.PriceCalculators
{
    public class ShippingDecorator : IPriceCalculator
    {
        private readonly IPriceCalculator _inner;
        private readonly decimal _shippingFee;

        public ShippingDecorator(IPriceCalculator inner, decimal shippingFee)
        {
            _inner = inner;
            _shippingFee = shippingFee;
        }

        public decimal Calculate(decimal basePrice)
        {
            return _inner.Calculate(basePrice) + _shippingFee;
        }
    }
}