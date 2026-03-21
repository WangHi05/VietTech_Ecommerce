namespace eCommerce.Application.Mediators
{
    // 1. Thành phần Giỏ hàng (Tính SubTotal)
    public class CartComponent : CheckoutComponent
    {
        public decimal SubTotal { get; private set; }
        
        public void SetSubTotal(decimal subTotal)
        {
            SubTotal = subTotal;
            _mediator?.Notify(this, "CartUpdated");
        }
    }

    // 2. Thành phần Vận chuyển (Tính ShippingFee)
    public class ShippingComponent : CheckoutComponent
    {
        public decimal Fee { get; private set; }
        
        public void SelectShippingMethod(string method)
        {
            // Logic tính ship (có thể gọi DB hoặc cấu hình cứng)
            Fee = method == "express" ? 50000m : 30000m;
            _mediator?.Notify(this, "ShippingUpdated");
        }
    }

    // 3. Thành phần Điểm thưởng & Voucher (Tính Discount)
    public class PromotionComponent : CheckoutComponent
    {
        public decimal PointsDiscount { get; private set; }
        public decimal VoucherDiscount { get; private set; }

        public void ApplyPoints(int pointsToRedeem)
        {
            PointsDiscount = pointsToRedeem * 1000; // Giả sử 1 điểm = 1000đ
            _mediator?.Notify(this, "PromotionUpdated");
        }

        public void ApplyVoucher(decimal discountAmount)
        {
            VoucherDiscount = discountAmount;
            _mediator?.Notify(this, "PromotionUpdated");
        }
    }
}