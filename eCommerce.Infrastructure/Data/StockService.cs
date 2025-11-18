using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Data
{
    public class StockService : IStockService
    {
        private readonly AppDbContext _context;

        public StockService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckStockAvailability(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            return product != null && product.StockQuantity >= quantity;
        }

        public async Task<bool> DeductStock(int productId, int quantity, int orderId, string userName = "")
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.StockQuantity < quantity)
                return false;

            var beforeQty = product.StockQuantity;
            product.StockQuantity -= quantity;

            // Ghi lịch sử xuất kho
            _context.StockHistories.Add(new StockHistory
            {
                ProductId = productId,
                Type = "Export",
                Quantity = -quantity, // Số âm để biểu thị giảm
                BeforeQuantity = beforeQty,
                AfterQuantity = product.StockQuantity,
                Reason = $"Xuất kho cho đơn hàng #{orderId}",
                OrderId = orderId,
                CreatedAt = DateTime.Now,
                CreatedBy = userName
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreStock(int productId, int quantity, int orderId, string reason, string userName = "")
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return false;

            var beforeQty = product.StockQuantity;
            product.StockQuantity += quantity;

            // Ghi lịch sử hoàn trả
            _context.StockHistories.Add(new StockHistory
            {
                ProductId = productId,
                Type = "Return",
                Quantity = quantity, // Số dương để biểu thị tăng
                BeforeQuantity = beforeQty,
                AfterQuantity = product.StockQuantity,
                Reason = reason,
                OrderId = orderId,
                CreatedAt = DateTime.Now,
                CreatedBy = userName
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ImportStock(int productId, int quantity, string reason, string userName)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return false;

            var beforeQty = product.StockQuantity;
            product.StockQuantity += quantity;

            // Ghi lịch sử nhập kho
            _context.StockHistories.Add(new StockHistory
            {
                ProductId = productId,
                Type = "Import",
                Quantity = quantity,
                BeforeQuantity = beforeQty,
                AfterQuantity = product.StockQuantity,
                Reason = reason,
                CreatedAt = DateTime.Now,
                CreatedBy = userName
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AdjustStock(int productId, int newQuantity, string reason, string userName)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return false;

            var beforeQty = product.StockQuantity;
            var difference = newQuantity - beforeQty;
            product.StockQuantity = newQuantity;

            // Ghi lịch sử điều chỉnh
            _context.StockHistories.Add(new StockHistory
            {
                ProductId = productId,
                Type = "Adjust",
                Quantity = difference,
                BeforeQuantity = beforeQty,
                AfterQuantity = product.StockQuantity,
                Reason = reason,
                CreatedAt = DateTime.Now,
                CreatedBy = userName
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Product>> GetLowStockProducts()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.StockQuantity < p.MinStockLevel)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        public async Task<List<StockHistory>> GetStockHistory(int productId)
        {
            return await _context.StockHistories
                .Include(sh => sh.Product)
                .Include(sh => sh.Order)
                .Where(sh => sh.ProductId == productId)
                .OrderByDescending(sh => sh.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<StockHistory>> GetAllStockHistory()
        {
            return await _context.StockHistories
                .Include(sh => sh.Product)
                .Include(sh => sh.Order)
                .OrderByDescending(sh => sh.CreatedAt)
                .Take(100) // Lấy 100 giao dịch gần nhất
                .ToListAsync();
        }
    }
}
