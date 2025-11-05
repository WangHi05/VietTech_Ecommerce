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
    }
}
