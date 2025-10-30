using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eCommerce.Application.Services
{
    public interface IOrderService
    {
        Task<int> PlaceOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<List<Order>> GetOrdersByUserAsync(string userId);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
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
    }
}
