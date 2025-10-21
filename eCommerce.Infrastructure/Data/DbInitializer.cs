using eCommerce.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(AppDbContext context)
        {
            // Chỉ seed khi không có sản phẩm nào
            if (context.Products.Any())
            {
                return; 
            }

            var categories = new Category[]
            {
                new Category{Name="Laptop", Description="Các loại máy tính xách tay mạnh mẽ"},
                new Category{Name="Bàn phím", Description="Bàn phím cơ và bàn phím văn phòng"},
                new Category{Name="Chuột máy tính", Description="Chuột chơi game và chuột văn phòng"},
                new Category{Name="Tai nghe", Description="Tai nghe không dây và có dây"}
            };
            
            if (!context.Categories.Any())
            {
                 await context.Categories.AddRangeAsync(categories);
                 await context.SaveChangesAsync();
            }

            var products = new Product[]
            {
                new Product
                {
                    Name="Laptop Gaming Pro X",
                    Description="Laptop chuyên game với cấu hình đỉnh cao",
                    Price=35000000,
                    StockQuantity=50,
                    // === CẬP NHẬT: THÊM URL HÌNH ẢNH ===
                    ImageUrl="/images/products/laptop1.jpg",
                    CategoryId=1, // Giả sử Id của Laptop là 1
                    Specifications = new Dictionary<string, string>
                    {
                        {"CPU", "Intel Core i9"}, {"RAM", "32GB DDR5"}, {"Ổ cứng", "1TB NVMe SSD"}, {"VGA", "NVIDIA RTX 4080"}
                    }
                },
                new Product
                {
                    Name="Bàn phím cơ TK-87",
                    Description="Bàn phím cơ Tenkeyless, switch Blue",
                    Price=1800000,
                    StockQuantity=100,
                    // === CẬP NHẬT: THÊM URL HÌNH ẢNH ===
                    ImageUrl="/images/products/banphim1.png",
                    CategoryId=2, // Giả sử Id của Bàn phím là 2
                    Specifications = new Dictionary<string, string>
                    {
                        {"Loại switch", "Blue Switch"}, {"Kết nối", "USB-C, Bluetooth 5.0"}, {"Layout", "Tenkeyless (87 phím)"}, {"LED", "RGB 16.8 triệu màu"}
                    }
                },
                new Product
                {
                    Name="Chuột không dây SilentMove",
                    Description="Chuột không click, phù hợp cho văn phòng",
                    Price=450000,
                    StockQuantity=200,
                    // === CẬP NHẬT: THÊM URL HÌNH ẢNH ===
                    ImageUrl="/images/products/chuot1.png",
                    CategoryId=3, // Giả sử Id của Chuột là 3
                    Specifications = new Dictionary<string, string>
                    {
                        {"DPI", "1600"}, {"Kết nối", "Wireless 2.4Ghz"}, {"Pin", "AA"}
                    }
                }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}