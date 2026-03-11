using eCommerce.Core.Entities;

namespace eCommerce.Application.DesignPatterns.Iterator
{
    /// Concrete Iterator - duyệt toàn bộ orders không filter
    public class OrderIterator : IIterator<Order>
    {
        private readonly List<Order> _orders;
        private int _position = -1;

        public OrderIterator(List<Order> orders)
        {
            _orders = orders;
        }

        public Order? Current
        {
            get
            {
                if (_position >= 0 && _position < _orders.Count)
                    return _orders[_position];
                return null;
            }
        }

        public bool MoveNext()
        {
            _position++;
            return _position < _orders.Count;
        }

        public void Reset()
        {
            _position = -1;
        }
    }
}
