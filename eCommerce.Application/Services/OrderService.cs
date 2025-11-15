using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eCommerce.Application.Services
{
    public interface IOrderService
    {
        Task<int> PlaceOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<List<Order>> GetOrdersByUserAsync(string userId);
        Task UpdatePaymentStateAsync(int orderId, string status, string paymentStatus, DateTime? paidAt = null);
    Task UpdateStatusAsync(int orderId, string status);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILoyaltyService? _loyaltyService;

        public OrderService(IOrderRepository orderRepository, ILoyaltyService? loyaltyService = null)
        {
            _orderRepository = orderRepository;
            _loyaltyService = loyaltyService;
        }

        public async Task<int> PlaceOrderAsync(Order order)
        {
            await _orderRepository.AddAsync(order);
            return order.Id;
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        public async Task<List<Order>> GetOrdersByUserAsync(string userId)
        {
            return await _orderRepository.GetByUserAsync(userId);
        }

        public async Task UpdatePaymentStateAsync(int orderId, string status, string paymentStatus, DateTime? paidAt = null)
        {
            await _orderRepository.UpdatePaymentStateAsync(orderId, status, paymentStatus, paidAt);
            
            // Tích điểm khi thanh toán thành công (Paid hoặc Succeeded)
            if ((status == "Paid" || paymentStatus == "Succeeded") && _loyaltyService != null)
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order != null && !string.IsNullOrEmpty(order.UserId))
                {
                    await _loyaltyService.AwardPointsForOrderAsync(order.UserId, orderId, order.Total);
                }
            }
        }

        public async Task UpdateStatusAsync(int orderId, string status)
        {
            await _orderRepository.UpdateStatusAsync(orderId, status);

            // Nếu đơn hàng hoàn thành, tự động tích điểm (cho trường hợp COD)
            if (status == "Hoàn thành" && _loyaltyService != null)
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order != null && !string.IsNullOrEmpty(order.UserId))
                {
                    await _loyaltyService.AwardPointsForOrderAsync(order.UserId, orderId, order.Total);
                }
            }
        }
    }
}
