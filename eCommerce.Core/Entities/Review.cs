using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Core.Entities
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public string? UserId { get; set; }

    // Related order (optional) - used to show per-order review status
    public int? OrderId { get; set; }

        [MaxLength(256)]
        public string? UserName { get; set; }

        [Range(1,5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        [MaxLength(32)]
        public string Status { get; set; } = "Pending";
    }
}
