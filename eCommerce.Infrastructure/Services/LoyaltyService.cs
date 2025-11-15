using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Services
{
    public class LoyaltyService : eCommerce.Application.Services.ILoyaltyService
    {
        private readonly AppDbContext _context;

        // Cấu hình quy tắc tích điểm
        private const decimal POINTS_PER_1000_VND = 1; // 1,000đ = 1 điểm
        private const decimal REDEMPTION_RATE = 50; // 50 điểm = 1,000đ

        // Ngưỡng hạng thành viên dựa trên LifetimePoints
        private const int SILVER_THRESHOLD = 1000;
        private const int GOLD_THRESHOLD = 5000;
        private const int DIAMOND_THRESHOLD = 10000;

        public LoyaltyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LoyaltyPoint?> GetOrCreateLoyaltyPointAsync(string userId)
        {
            var loyaltyPoint = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(lp => lp.UserId == userId);

            if (loyaltyPoint == null)
            {
                loyaltyPoint = new LoyaltyPoint
                {
                    UserId = userId,
                    TotalPoints = 0,
                    LifetimePoints = 0,
                    MembershipTier = "Bronze",
                    LastUpdated = DateTime.Now
                };
                _context.LoyaltyPoints.Add(loyaltyPoint);
                await _context.SaveChangesAsync();
            }

            return loyaltyPoint;
        }

        public int CalculatePointsFromOrder(decimal orderTotal)
        {
            // 1,000đ = 1 điểm
            return (int)(orderTotal / 1000 * POINTS_PER_1000_VND);
        }

        public async Task<bool> AwardPointsForOrderAsync(string userId, int orderId, decimal orderTotal)
        {
            try
            {
                // Kiểm tra xem đã tích điểm cho đơn hàng này chưa
                var existingTransaction = await _context.PointTransactions
                    .FirstOrDefaultAsync(t => t.OrderId == orderId && t.Type == "Earn");
                
                if (existingTransaction != null)
                {
                    // Đã tích điểm rồi, không tích lại
                    return false;
                }

                var loyaltyPoint = await GetOrCreateLoyaltyPointAsync(userId);
                if (loyaltyPoint == null) return false;

                int pointsToAdd = CalculatePointsFromOrder(orderTotal);

                // Cộng điểm
                loyaltyPoint.TotalPoints += pointsToAdd;
                loyaltyPoint.LifetimePoints += pointsToAdd;
                loyaltyPoint.LastUpdated = DateTime.Now;

                // Cập nhật hạng thành viên
                loyaltyPoint.MembershipTier = GetMembershipTier(loyaltyPoint.LifetimePoints);

                // Tạo transaction log
                var transaction = new PointTransaction
                {
                    UserId = userId,
                    OrderId = orderId,
                    Points = pointsToAdd,
                    Type = "Earn",
                    Description = $"Tích điểm từ đơn hàng #{orderId}",
                    CreatedAt = DateTime.Now
                };
                _context.PointTransactions.Add(transaction);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool Success, decimal DiscountAmount)> RedeemPointsAsync(string userId, int pointsToRedeem)
        {
            try
            {
                var loyaltyPoint = await GetOrCreateLoyaltyPointAsync(userId);
                if (loyaltyPoint == null) return (false, 0);

                if (loyaltyPoint.TotalPoints < pointsToRedeem)
                {
                    return (false, 0);
                }

                // Tính số tiền giảm giá: 50 điểm = 1,000đ
                decimal discountAmount = (pointsToRedeem / REDEMPTION_RATE) * 1000;

                // Trừ điểm
                loyaltyPoint.TotalPoints -= pointsToRedeem;
                loyaltyPoint.LastUpdated = DateTime.Now;

                // Tạo transaction log
                var transaction = new PointTransaction
                {
                    UserId = userId,
                    Points = -pointsToRedeem,
                    Type = "Redeem",
                    Description = $"Sử dụng {pointsToRedeem} điểm để giảm {discountAmount:N0}đ",
                    CreatedAt = DateTime.Now
                };
                _context.PointTransactions.Add(transaction);

                await _context.SaveChangesAsync();
                return (true, discountAmount);
            }
            catch
            {
                return (false, 0);
            }
        }

        public async Task<List<PointTransaction>> GetTransactionHistoryAsync(string userId, int take = 20)
        {
            return await _context.PointTransactions
                .Where(pt => pt.UserId == userId)
                .OrderByDescending(pt => pt.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public string GetMembershipTier(int lifetimePoints)
        {
            if (lifetimePoints >= DIAMOND_THRESHOLD) return "Diamond";
            if (lifetimePoints >= GOLD_THRESHOLD) return "Gold";
            if (lifetimePoints >= SILVER_THRESHOLD) return "Silver";
            return "Bronze";
        }

        public (string NextTier, int PointsNeeded, double ProgressPercent) GetNextTierInfo(int lifetimePoints)
        {
            if (lifetimePoints >= DIAMOND_THRESHOLD)
            {
                return ("Diamond", 0, 100);
            }
            else if (lifetimePoints >= GOLD_THRESHOLD)
            {
                int pointsNeeded = DIAMOND_THRESHOLD - lifetimePoints;
                double progress = ((double)(lifetimePoints - GOLD_THRESHOLD) / (DIAMOND_THRESHOLD - GOLD_THRESHOLD)) * 100;
                return ("Diamond", pointsNeeded, progress);
            }
            else if (lifetimePoints >= SILVER_THRESHOLD)
            {
                int pointsNeeded = GOLD_THRESHOLD - lifetimePoints;
                double progress = ((double)(lifetimePoints - SILVER_THRESHOLD) / (GOLD_THRESHOLD - SILVER_THRESHOLD)) * 100;
                return ("Gold", pointsNeeded, progress);
            }
            else
            {
                int pointsNeeded = SILVER_THRESHOLD - lifetimePoints;
                double progress = ((double)lifetimePoints / SILVER_THRESHOLD) * 100;
                return ("Silver", pointsNeeded, progress);
            }
        }

        public string GetTierIcon(string tier)
        {
            return tier switch
            {
                "Diamond" => "💎",
                "Gold" => "🥇",
                "Silver" => "🥈",
                "Bronze" => "🥉",
                _ => "🥉"
            };
        }

        public string GetTierColor(string tier)
        {
            return tier switch
            {
                "Diamond" => "#b9f2ff",
                "Gold" => "#ffd700",
                "Silver" => "#c0c0c0",
                "Bronze" => "#cd7f32",
                _ => "#cd7f32"
            };
        }
    }
}
