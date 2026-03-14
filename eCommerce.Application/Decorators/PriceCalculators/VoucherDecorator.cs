namespace eCommerce.Application.Decorators.PriceCalculators
{
    public class VoucherDecorator : IPriceCalculator
    {
        private readonly IPriceCalculator _inner;
        private readonly decimal _discountAmount;

        public VoucherDecorator(IPriceCalculator inner, decimal discountAmount)
        {
            _inner = inner;
            _discountAmount = discountAmount;
        }

        public decimal Calculate(decimal basePrice)
        {
            // Tính giá sau các lớp bọc trước đó rồi trừ đi tiền Voucher
            return _inner.Calculate(basePrice) - _discountAmount;
        }
    }
}