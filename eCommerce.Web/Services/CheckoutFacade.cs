using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Web.Services
{
    // FACADE PATTERN: Che giấu logic phức tạp của 5 subsystems
    public class CheckoutFacade : ICheckoutFacade
    {
        private readonly IOrderService _orderService;
        private readonly IStockService _stockService;
        private readonly ILoyaltyService _loyaltyService;
        private readonly ICartService _cartService;
        private readonly AppDbContext _context;

        public CheckoutFacade(
            IOrderService orderService,
            IStockService stockService,
            ILoyaltyService loyaltyService,
            ICartService cartService,
            AppDbContext context)
        {
            _orderService = orderService;
            _stockService = stockService;
            _loyaltyService = loyaltyService;
            _cartService = cartService;
            _context = context;
        }

        public async Task<CheckoutResult> PlaceOrderAsync(CheckoutRequest request)
        {
            foreach (var item in request.Items)
            {
                var hasStock = await _stockService.CheckStockAvailability(item.ProductId, item.Quantity);
                if (!hasStock)
                {
                    return new CheckoutResult
                    {
                        Success = false,
                        ErrorMessage = $"Sản phẩm '{item.Name}' không đủ số lượng trong kho."
                    };
                }
            }

            var order = BuildOrder(request);
            var orderId = await _orderService.PlaceOrderAsync(order);

            foreach (var item in request.Items)
            {
                await _stockService.DeductStock(
                    item.ProductId, item.Quantity, orderId, request.UserName);
            }

            if (request.PointsToRedeem > 0 && !string.IsNullOrEmpty(request.UserId))
            {
                await _loyaltyService.RedeemPointsAsync(request.UserId, request.PointsToRedeem);
            }

            await ProcessVoucherAfterOrderAsync(orderId);

            if (request.ClearCart)
            {
                await _cartService.ClearCartAsync();
            }

            return new CheckoutResult { Success = true, OrderId = orderId };
        }

        private static Order BuildOrder(CheckoutRequest req)
        {
            var order = new Order
            {
                UserId           = req.UserId,
                CreatedAt        = DateTime.UtcNow,
                PaymentMethod    = req.PaymentMethod,
                PaymentStatus    = req.InitialPaymentStatus,
                Status           = req.InitialStatus,
                ShippingName     = req.ShippingName,
                ShippingAddress  = req.ShippingAddress,
                ShippingCountry  = req.ShippingCountry,
                ShippingProvince = req.ShippingProvince,
                ShippingMethod   = req.ShippingMethod,
                CardHolderName   = req.CardHolderName,
                CardLast4        = req.CardLast4,
                SubTotal         = req.SubTotal,
                Discount         = req.Discount,
                ShippingFee      = req.ShippingFee,
                VoucherCode      = req.VoucherCode,
                Total            = req.Total
            };

            foreach (var item in req.Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Name      = item.Name,
                    Price     = item.Price,
                    Quantity  = item.Quantity
                });
            }

            return order;
        }

        private async Task ProcessVoucherAfterOrderAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
                if (order == null || string.IsNullOrEmpty(order.VoucherCode)) return;

                var voucher = await _context.Vouchers
                    .FirstOrDefaultAsync(v => v.Code == order.VoucherCode);
                if (voucher == null) return;

                if (!string.IsNullOrEmpty(order.UserId))
                {
                    var uv = await _context.UserVouchers
                        .FirstOrDefaultAsync(x => x.UserId == order.UserId && x.VoucherId == voucher.Id);
                    if (uv != null) _context.UserVouchers.Remove(uv);
                }

                voucher.UsedCount++;
                if (voucher.MaxUsage <= voucher.UsedCount)
                    _context.Vouchers.Remove(voucher);

                await _context.SaveChangesAsync();
            }
            catch { }
        }
    }
}
