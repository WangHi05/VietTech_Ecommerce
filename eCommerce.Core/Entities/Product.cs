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

        // === CÁC TRƯỜNG MỚI ĐỂ LỌC ===

        // Foreign key cho Brand
        [Required(ErrorMessage = "Thương hiệu là bắt buộc")]
        public int? BrandId { get; set; }
        // Navigation property cho Brand
        public Brand? Brand { get; set; }

        [MaxLength(50)]
        public string Color { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Size { get; set; } = string.Empty;

        public bool IsNew { get; set; }
        public bool IsOnSale { get; set; }
        public decimal? OldPrice { get; set; }
        public int DiscountPercent { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        

        // === HẾT CÁC TRƯỜNG MỚI ===

        // Thuộc tính để lưu các thông số kỹ thuật dưới dạng JSON
        public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();
    }
}
