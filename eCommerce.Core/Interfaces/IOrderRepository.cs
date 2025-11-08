using eCommerce.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eCommerce.Core.Interfaces
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        Task<Order?> GetByIdAsync(int id);
        Task<List<Order>> GetByUserAsync(string userId);
        Task UpdatePaymentStateAsync(int orderId, string status, string paymentStatus, DateTime? paidAt = null);

        // 1. Lấy tất cả đơn hàng (kèm theo thông tin khách hàng)
        Task<List<Order>> GetAllForAdminAsync();

        // 2. Lấy chi tiết 1 đơn hàng (kèm khách hàng VÀ các sản phẩm trong đơn)
        Task<Order?> GetDetailsByIdForAdminAsync(int id);
    }
}
