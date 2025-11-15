namespace eCommerce.Core.Entities
{
    public class PointTransaction
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? OrderId { get; set; }
        public int Points { get; set; } // Dương = tích điểm, Âm = tiêu điểm
        public string Type { get; set; } = string.Empty; // Earn, Redeem, Bonus, Expired
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ApplicationUser? User { get; set; }
        public Order? Order { get; set; }
    }
}
