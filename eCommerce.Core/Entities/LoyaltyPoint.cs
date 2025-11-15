namespace eCommerce.Core.Entities
{
    public class LoyaltyPoint
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int TotalPoints { get; set; } // Điểm hiện có
        public int LifetimePoints { get; set; } // Tổng điểm tích lũy từ trước đến nay
        public string MembershipTier { get; set; } = "Bronze"; // Bronze, Silver, Gold, Diamond
        public DateTime LastUpdated { get; set; }

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}
