using eCommerce.Application.Flyweights;

namespace eCommerce.Application.ViewModels
{
    public class ProductViewModel
    {
        // 1. Dữ liệu riêng biệt (Extrinsic State) - Tốn bộ nhớ riêng cho từng sản phẩm
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        // 2. Tham chiếu chia sẻ (Intrinsic State) - Tiết kiệm bộ nhớ!
        public BrandFlyweight BrandInfo { get; set; }

        public ProductViewModel(int id, string name, decimal price, string imageUrl, BrandFlyweight brandInfo)
        {
            Id = id;
            Name = name;
            Price = price;
            ImageUrl = imageUrl;
            BrandInfo = brandInfo;
        }
    }
}