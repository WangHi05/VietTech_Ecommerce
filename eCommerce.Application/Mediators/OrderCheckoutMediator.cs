using eCommerce.Application.Decorators.PriceCalculators;

namespace eCommerce.Application.Mediators
{
    public class OrderCheckoutMediator : ICheckoutMediator
    {
        private readonly CartComponent _cart;
        private readonly ShippingComponent _shipping;
        private readonly PromotionComponent _promotion;

        // Kết quả cuối cùng
        public decimal FinalTotal { get; private set; }

        public OrderCheckoutMediator(CartComponent cart, ShippingComponent shipping, PromotionComponent promotion)
        {
            _cart = cart;         _cart.SetMediator(this);
            _shipping = shipping; _shipping.SetMediator(this);
            _promotion = promotion; _promotion.SetMediator(this);
        }

        // Khi có bất kỳ ai báo cáo sự thay đổi, tính lại tiền ngay!
        public void Notify(CheckoutComponent sender, string eventCode)
        {
            CalculateFinalPrice();
        }

        private void CalculateFinalPrice()
        {
            // TÍCH HỢP DECORATOR PATTERN ĐỂ TÍNH TIỀN
            IPriceCalculator calculator = new BasePriceCalculator();

            if (_promotion.VoucherDiscount > 0)
                calculator = new VoucherDecorator(calculator, _promotion.VoucherDiscount);

            if (_promotion.PointsDiscount > 0)
                calculator = new LoyaltyDecorator(calculator, _promotion.PointsDiscount);

            calculator = new ShippingDecorator(calculator, _shipping.Fee);

            FinalTotal = calculator.Calculate(_cart.SubTotal);
            
            // Đảm bảo tổng tiền không bị âm
            if (FinalTotal < 0) FinalTotal = 0; 
        }
    }
}