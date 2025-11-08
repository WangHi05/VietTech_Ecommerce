using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Core.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser? Customer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string PaymentMethod { get; set; } = "COD";
        public string PaymentStatus { get; set; } = "Pending";
        public string Status { get; set; } = "Pending";
        public DateTime? PaidAt { get; set; }
        public string? CardHolderName { get; set; }
        public string? CardLast4 { get; set; }

        // Shipping info
        public string ShippingName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;
        public string ShippingProvince { get; set; } = string.Empty;

        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingFee { get; set; }
        public string? VoucherCode { get; set; }
        public decimal Total { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
