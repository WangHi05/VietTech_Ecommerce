using System.ComponentModel.DataAnnotations;

namespace eCommerce.Core.Entities
{
    public class Voucher
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        // Percentage discount (0-100) or null if using DiscountAmount
        public decimal? DiscountPercent { get; set; }

        // Fixed discount amount or null if using DiscountPercent
        public decimal? DiscountAmount { get; set; }

        // Minimum order value to use this voucher
        public decimal MinOrderValue { get; set; } = 0;

        // Maximum discount amount (cap for percentage discounts)
        public decimal? MaxDiscountAmount { get; set; }

        // Maximum number of times this voucher can be used (total)
        public int MaxUsage { get; set; } = 1000;

        // How many times this voucher has been used
        public int UsedCount { get; set; } = 0;

        // Maximum times one user can use this voucher
        public int MaxUsagePerUser { get; set; } = 1;

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
    }
}
