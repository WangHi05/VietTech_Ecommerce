using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using System.Threading.Tasks;
using eCommerce.Application.Services;

namespace eCommerce.Application.Observers
{
    public class LoyaltyOrderObserver : IOrderObserver
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyOrderObserver(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        public async Task OrderPaymentStatusChangedAsync(Order order, string paymentStatus)
        {
            if (paymentStatus == "Đã thanh toán" && order != null && !string.IsNullOrEmpty(order.UserId))
            {
                await _loyaltyService.AwardPointsForOrderAsync(order.UserId, order.Id, order.Total);
            }
        }

        public async Task OrderStatusChangedAsync(Order order, string status)
        {
            if (status == "Hoàn thành" && order != null && !string.IsNullOrEmpty(order.UserId))
            {
                await _loyaltyService.AwardPointsForOrderAsync(order.UserId, order.Id, order.Total);
            }
        }
    }
}