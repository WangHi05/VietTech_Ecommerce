using eCommerce.Core.Entities;

namespace eCommerce.Web.Services
{
    public class CheckoutRequest
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public string ShippingName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;
        public string ShippingProvince { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = "standard";
        public string? CardHolderName { get; set; }
        public string? CardLast4 { get; set; }
        public string? UserId { get; set; }
        public string UserName { get; set; } = "Guest";
        public int PointsToRedeem { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal PointsDiscount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public string? VoucherCode { get; set; }
        public string InitialPaymentStatus { get; set; } = "Chưa thanh toán";
        public string InitialStatus { get; set; } = "Đang chờ";
        public bool ClearCart { get; set; } = true;
    }

    public class CheckoutResult
    {
        public bool Success { get; set; }
        public int OrderId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // FACADE PATTERN: Giao diện đơn giản cho toàn bộ quy trình đặt hàng
    public interface ICheckoutFacade
    {
        Task<CheckoutResult> PlaceOrderAsync(CheckoutRequest request);
    }
}
