using System.Collections.Generic;

namespace eCommerce.Core.Entities
{
    // Bảng mới để lưu Thương hiệu
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        // Một thương hiệu có nhiều sản phẩm
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
