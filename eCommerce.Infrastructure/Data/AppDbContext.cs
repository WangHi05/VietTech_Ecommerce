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
        public DbSet<eCommerce.Core.Entities.Review> Reviews { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<eCommerce.Core.Entities.Order> Orders { get; set; }
        public DbSet<eCommerce.Core.Entities.OrderItem> OrderItems { get; set; }
        public DbSet<eCommerce.Core.Entities.UserPushSubscription> UserPushSubscriptions { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }
        public DbSet<PointTransaction> PointTransactions { get; set; }
        public DbSet<StockHistory> StockHistories { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<UserVoucher> UserVouchers { get; set; }
        public DbSet<Message> Messages { get; set; }
       
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
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<eCommerce.Core.Entities.Order>()
                .Property(o => o.PaymentMethod)
                .HasMaxLength(32)
                .HasDefaultValue("COD");

            modelBuilder.Entity<eCommerce.Core.Entities.Order>()
                .Property(o => o.PaymentStatus)
                .HasMaxLength(32)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<eCommerce.Core.Entities.Order>()
                .Property(o => o.Status)
                .HasMaxLength(32)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<eCommerce.Core.Entities.Order>()
                .Property(o => o.CardLast4)
                .HasMaxLength(8);

            modelBuilder.Entity<eCommerce.Core.Entities.Order>()
                .Property(o => o.CardHolderName)
                .HasMaxLength(128);

            // === CẤU HÌNH QUAN HỆ MỚI ===
            // Cấu hình quan hệ Product và Brand (Một-Nhiều)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId);

            // Cấu hình LoyaltyPoint
            modelBuilder.Entity<LoyaltyPoint>()
                .HasOne(lp => lp.User)
                .WithMany()
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LoyaltyPoint>()
                .HasIndex(lp => lp.UserId)
                .IsUnique();

            // Cấu hình PointTransaction
            modelBuilder.Entity<PointTransaction>()
                .HasOne(pt => pt.User)
                .WithMany()
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PointTransaction>()
                .HasOne(pt => pt.Order)
                .WithMany()
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
            
           
            
        }
    }
}