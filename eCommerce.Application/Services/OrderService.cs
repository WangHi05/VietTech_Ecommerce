using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eCommerce.Application.Services
{
    // Cập nhật interface kế thừa thêm IOrderSubject
    public interface IOrderService : IOrderSubject
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
        
        // Danh sách các Observer đang theo dõi Service này
        private readonly List<IOrderObserver> _observers = new List<IOrderObserver>();

        // Chỉ cần giữ lại IOrderRepository
        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // --- Triển khai các hàm của IOrderSubject ---
        public void Attach(IOrderObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void Detach(IOrderObserver observer)
        {
            _observers.Remove(observer);
        }

        public async Task NotifyPaymentStatusChangedAsync(Order order, string paymentStatus)
        {
            foreach (var observer in _observers)
            {
                await observer.OrderPaymentStatusChangedAsync(order, paymentStatus);
            }
        }

        public async Task NotifyStatusChangedAsync(Order order, string status)
        {
            foreach (var observer in _observers)
            {
                await observer.OrderStatusChangedAsync(order, status);
            }
        }
        // ---------------------------------------------

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
            
            // Lấy order và thông báo cho các Observers thay vì gọi trực tiếp logic
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                await NotifyPaymentStatusChangedAsync(order, paymentStatus);
            }
        }

        public async Task UpdateStatusAsync(int orderId, string status)
        {
            await _orderRepository.UpdateStatusAsync(orderId, status);

            // Lấy order và thông báo cho các Observers
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                await NotifyStatusChangedAsync(order, status);
            }
        }
    }
}