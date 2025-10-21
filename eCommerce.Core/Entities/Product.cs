using System.Collections.Generic;

namespace eCommerce.Core.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
        // Foreign key cho Category
        public int CategoryId { get; set; }
        // Navigation property cho Category
        public Category? Category { get; set; }

        // Thuộc tính để lưu các thông số kỹ thuật dưới dạng JSON
        public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();
    }
}
