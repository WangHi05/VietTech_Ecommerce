using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Identity; // Thêm using
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce.Infrastructure.Data
{
    public static class DbInitializer
    {
        // Cập nhật phương thức Initialize để nhận UserManager và RoleManager
        public static async Task Initialize(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // === 1. SEED ROLES ===
            string adminRole = "Admin";
            string customerRole = "Customer";

            if (!context.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
                await roleManager.CreateAsync(new IdentityRole(customerRole));
            }

            // === 2. SEED ADMIN USER ===
            if (!context.Users.Any(u => u.Email == "dinhquanghuy6300@gmail.com"))
            {
                // !!! THAY ĐỔI EMAIL VÀ MẬT KHẨU NÀY !!!
                var adminEmail = "dinhquanghuy6300@gmail.com";
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Admin" // Thêm FullName
                };

                // Tạo user với mật khẩu
                await userManager.CreateAsync(adminUser, "Admin@123"); 
                
                // Gán role Admin cho user này
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }

            // === 3. SEED CATEGORIES ===
            if (!context.Categories.Any())
            {
                var categories = new Category[]
                {
                    new Category{Name="Laptop", Description="Các loại máy tính xách tay mạnh mẽ"},
                    new Category{Name="Bàn phím", Description="Bàn phím cơ và bàn phím văn phòng"},
                    new Category{Name="Chuột máy tính", Description="Chuột chơi game và chuột văn phòng"},
                    new Category{Name="Tai nghe", Description="Tai nghe không dây và có dây"}
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // === 4. SEED BRANDS (MỚI) ===
            if (!context.Brands.Any())
            {
                var brands = new Brand[]
                {
                    new Brand { Name = "Dell" },
                    new Brand { Name = "HP" },
                    new Brand { Name = "Logitech" },
                    new Brand { Name = "Sony" },
                    new Brand { Name = "Apple" },
                    new Brand { Name = "Samsung" },
                    new Brand { Name = "Razer" },
                    new Brand { Name = "Keychron" },
                    new Brand { Name = "Akko" },
                    new Brand { Name = "Custom" },
                    new Brand { Name = "Asus" },
                    new Brand { Name = "Beats" },
                    new Brand { Name = "Glorious" },
                    new Brand { Name = "Generic" }
                };
                await context.Brands.AddRangeAsync(brands);
                await context.SaveChangesAsync();
            }


            // === 5. SEED PRODUCTS (CẬP NHẬT) ===
            // Chỉ seed khi không có sản phẩm nào
            if (context.Products.Any())
            {
                return;
            }

            // Lấy Id của category và brand để gán
            int laptopCatId = context.Categories.First(c => c.Name == "Laptop").Id;
            int keyboardCatId = context.Categories.First(c => c.Name == "Bàn phím").Id;
            int mouseCatId = context.Categories.First(c => c.Name == "Chuột máy tính").Id;
            int headphoneCatId = context.Categories.First(c => c.Name == "Tai nghe").Id;

            int dellBrandId = context.Brands.First(b => b.Name == "Dell").Id;
            int logitechBrandId = context.Brands.First(b => b.Name == "Logitech").Id;
            int sonyBrandId = context.Brands.First(b => b.Name == "Sony").Id;

            var products = new Product[]
            {
                new Product
                {
                    Name="Laptop Dell XPS 15",
                    Description="Laptop chuyên game với cấu hình đỉnh cao",
                    Price=35000000,
                    StockQuantity=50,
                    ImageUrl="/images/products/laptop1.jpg",
                    CategoryId = laptopCatId,
                    BrandId = dellBrandId, // Cập nhật
                    Color = "Silver", // Cập nhật
                    Size = "15 inch", // Cập nhật
                    Specifications = new Dictionary<string, string>
                    {
                        {"CPU", "Intel Core i9"}, {"RAM", "32GB DDR5"}, {"Ổ cứng", "1TB NVMe SSD"}, {"VGA", "NVIDIA RTX 4080"}
                    }
                },
                new Product
                {
                    Name="Bàn phím cơ Logitech G Pro",
                    Description="Bàn phím cơ Tenkeyless, switch Blue",
                    Price=1800000,
                    StockQuantity=100,
                    ImageUrl="/images/products/banphim1.png",
                    CategoryId = keyboardCatId,
                    BrandId = logitechBrandId, // Cập nhật
                    Color = "Black", // Cập nhật
                    Size = "Tenkeyless", // Cập nhật
                    Specifications = new Dictionary<string, string>
                    {
                        {"Loại switch", "Blue Switch"}, {"Kết nối", "USB-C, Bluetooth 5.0"}, {"Layout", "Tenkeyless (87 phím)"}, {"LED", "RGB 16.8 triệu màu"}
                    }
                },
                new Product
                {
                    Name="Chuột Logitech MX Master 3",
                    Description="Chuột không click, phù hợp cho văn phòng",
                    Price=450000,
                    StockQuantity=200,
                    ImageUrl="/images/products/chuot1.png",
                    CategoryId = mouseCatId,
                    BrandId = logitechBrandId, // Cập nhật
                    Color = "Graphite", // Cập nhật
                    Size = "Standard", // Cập nhật
                    Specifications = new Dictionary<string, string>
                    {
                        {"DPI", "1600"}, {"Kết nối", "Wireless 2.4Ghz"}, {"Pin", "AA"}
                    }
                },
                new Product
                {
                    Name="Tai nghe Sony WH-1000XM5",
                    Description="Tai nghe chống ồn chủ động, âm thanh Hi-Res",
                    Price=3200000,
                    StockQuantity=80,
                    ImageUrl="/images/products/tainghe1.png",
                    CategoryId = headphoneCatId,
                    BrandId = sonyBrandId, // Cập nhật
                    Color = "Black", // Cập nhật
                    Size = "Over-ear", // Cập nhật
                    Specifications = new Dictionary<string, string>
                    {
                        {"Chống ồn", "Active Noise Cancellation"}, {"Kết nối", "Bluetooth 5.2, Jack 3.5mm"}, {"Thời lượng pin", "40 giờ"}
                    }
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}