using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace eCommerce.Core.Entities
{
    public class Product : ICloneable
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải là số dương")]
        [Column(TypeName = "decimal(18,2)")] // Giúp CSDL lưu trữ chính xác
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không thể là số âm")]
        public int StockQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Mức tồn kho tối thiểu không thể là số âm")]
        public int MinStockLevel { get; set; } = 10; // Cảnh báo khi tồn kho thấp hơn giá trị này

        public bool IsAvailable => StockQuantity > 0; // Tự động tính còn hàng hay không

        public string ImageUrl { get; set; } = string.Empty;

        // Foreign key cho Category
        public int CategoryId { get; set; }
        // Navigation property cho Category
        [JsonIgnore] //tránh vòng lặp Json
        public Category? Category { get; set; }

        // === CÁC TRƯỜNG MỚI ĐỂ LỌC ===

        // Foreign key cho Brand
        [Required(ErrorMessage = "Thương hiệu là bắt buộc")]
        public int? BrandId { get; set; }
        // Navigation property cho Brand
        [JsonIgnore]
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
        
        [NotMapped]
        public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();

    //--------------------------Prototype----------------------

        public object Clone()
        {
            // (Shallow Copy) cho các kiểu dữ liệu giá trị (int, string, decimal)
            var clone = (Product)this.MemberwiseClone();
            
            // DEEP COPY CHO SPECIFICATIONS
            // tạo một Dictionary mới hoàn toàn và chép dữ liệu từ cái cũ sang
            if (this.Specifications != null)
            {
                clone.Specifications = new Dictionary<string, string>(this.Specifications);
            }
            else
            {
                clone.Specifications = new Dictionary<string, string>();
            }

            // 3. Reset các thông tin để Database chấp nhận là bản ghi mới
            clone.Id = 0;
            clone.Category = null; // Ngắt vòng lặp JSON
            clone.Brand = null;
            
            return clone;
        }
    //--------------------------Prototype-----------------------
    }
}