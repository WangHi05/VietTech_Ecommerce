using eCommerce.Core.Entities;

namespace eCommerce.Application.DesignPatterns.Iterator
{
    /// <summary>
    /// Concrete Aggregate - chứa danh sách orders và tạo các loại iterator
    /// </summary>
    public class OrderCollection : IAggregate<Order>
    {
        private readonly List<Order> _orders = new();

        public void AddOrder(Order order)
        {
            _orders.Add(order);
        }

        public void RemoveOrder(Order order)
        {
            _orders.Remove(order);
        }

        public int Count => _orders.Count;

        // Tạo iterator duyệt toàn bộ
        public IIterator<Order> CreateIterator()
        {
            return new OrderIterator(_orders);
        }

        // Tạo iterator duyệt theo status
        public IIterator<Order> CreateStatusIterator(string status)
        {
            return new OrderStatusIterator(_orders, status);
        }

        // Tạo iterator duyệt theo payment status
        public IIterator<Order> CreatePaymentIterator(string paymentStatus)
        {
            return new OrderPaymentStatusIterator(_orders, paymentStatus);
        }

        // Tạo iterator duyệt theo date range
        public IIterator<Order> CreateDateRangeIterator(DateTime fromDate, DateTime toDate)
        {
            return new OrderDateRangeIterator(_orders, fromDate, toDate);
        }
    }
}
