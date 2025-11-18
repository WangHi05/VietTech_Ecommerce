using System.ComponentModel.DataAnnotations;

namespace eCommerce.Core.Entities
{
    public class UserVoucher
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int VoucherId { get; set; }

        public DateTime CollectedDate { get; set; } = DateTime.Now;

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedDate { get; set; }

        public int? OrderId { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public Voucher Voucher { get; set; } = null!;
        public Order? Order { get; set; }
    }
}
