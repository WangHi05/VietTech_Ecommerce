using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Thêm using này
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
        // Không cần khai báo DbSet<ApplicationUser>, IdentityDbContext đã làm điều đó

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Dòng này rất quan trọng khi dùng Identity

            modelBuilder.Entity<Product>()
                .Property(p => p.Specifications)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, string>()
                );
        }
    }
}