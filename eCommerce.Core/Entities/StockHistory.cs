using System;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.Core.Entities
{
    public class StockHistory
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty; // "Import" (nhập), "Export" (xuất), "Adjust" (điều chỉnh), "Return" (hoàn trả)

        [Required]
        public int Quantity { get; set; } // Số lượng thay đổi (dương hoặc âm)

        [Required]
        public int BeforeQuantity { get; set; } // Số lượng trước khi thay đổi

        [Required]
        public int AfterQuantity { get; set; } // Số lượng sau khi thay đổi

        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty; // Lý do thay đổi

        public int? OrderId { get; set; } // Nếu liên quan đến đơn hàng
        public Order? Order { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(256)]
        public string CreatedBy { get; set; } = string.Empty; // User thực hiện thay đổi
    }
}
