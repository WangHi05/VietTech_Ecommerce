using eCommerce.Core.Entities;

namespace eCommerce.Application.Services
{
    public interface ILoyaltyService
    {
        Task<LoyaltyPoint?> GetOrCreateLoyaltyPointAsync(string userId);
        int CalculatePointsFromOrder(decimal orderTotal);
        Task<bool> AwardPointsForOrderAsync(string userId, int orderId, decimal orderTotal);
        Task<(bool Success, decimal DiscountAmount)> RedeemPointsAsync(string userId, int pointsToRedeem);
        Task<List<PointTransaction>> GetTransactionHistoryAsync(string userId, int take = 20);
        string GetMembershipTier(int lifetimePoints);
        (string NextTier, int PointsNeeded, double ProgressPercent) GetNextTierInfo(int lifetimePoints);
        string GetTierIcon(string tier);
        string GetTierColor(string tier);
    }
}
