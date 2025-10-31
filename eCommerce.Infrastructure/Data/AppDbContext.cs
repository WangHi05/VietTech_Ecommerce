using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace eCommerce.Infrastructure.Data
{
    // Kế thừa từ IdentityDbContext thay vì DbContext
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<eCommerce.Core.Entities.Order> Orders { get; set; }
        public DbSet<eCommerce.Core.Entities.OrderItem> OrderItems { get; set; }
        
       
        public DbSet<Brand> Brands { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Rất quan trọng khi dùng Identity

            modelBuilder.Entity<Product>()
                .Property(p => p.Specifications)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                );
            
            // Order -> OrderItems relationship
            modelBuilder.Entity<eCommerce.Core.Entities.Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // === CẤU HÌNH QUAN HỆ MỚI ===
            // Cấu hình quan hệ Product và Brand (Một-Nhiều)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId);
            
           
            
        }
    }
}