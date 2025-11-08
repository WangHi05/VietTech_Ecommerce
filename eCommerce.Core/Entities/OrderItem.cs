using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Core.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        [ForeignKey("OrderId")] 
        public Order? Order { get; set; } 

        public int ProductId { get; set; }
        [ForeignKey("ProductId")] 
        public Product? Product { get; set; } 

        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal => Price * Quantity;
    }
}
