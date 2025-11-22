namespace eCommerce.Core.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string SenderId { get; set; } = string.Empty; // UserId
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } // Cho phép gửi ảnh
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsFromSeller { get; set; } // true = seller, false = customer
        
        // Navigation
        public virtual Product? Product { get; set; }
    }
}
