using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eCommerce.Infrastructure.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Order>> GetAllForAdminAsync()
        {
            // Lấy tất cả đơn hàng
            // Sắp xếp theo ngày mới nhất lên đầu
            // Dùng Include() để lấy luôn thông tin Customer
            return await _context.Orders
                .Include(o => o.Customer) 
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetDetailsByIdForAdminAsync(int id)
        {
            // Lấy 1 đơn hàng theo Id
            // Dùng Include() để lấy thông tin Customer
            // Dùng Include() và ThenInclude() để lấy danh sách sản phẩm (Items)
            // và thông tin chi tiết của từng sản phẩm đó
            return await _context.Orders
                .Include(o => o.Customer) // Lấy thông tin khách hàng
                .Include(o => o.Items)    // Lấy danh sách các mục trong đơn
                    .ThenInclude(item => item.Product) // Lấy thông tin sản phẩm của từng mục
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetByUserAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task UpdatePaymentStateAsync(int orderId, string status, string paymentStatus, DateTime? paidAt = null)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return;
            }

            order.Status = status;
            order.PaymentStatus = paymentStatus;
            order.PaidAt = paidAt;

            await _context.SaveChangesAsync();
        }
    }
}
