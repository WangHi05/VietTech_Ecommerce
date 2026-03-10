using eCommerce.Core.Entities;

namespace eCommerce.Application.DesignPatterns.Iterator
{
    /// <summary>
    /// Concrete Iterator - duyệt orders theo status
    /// Không tạo list mới, duyệt trực tiếp và skip phần tử không match
    /// </summary>
    public class OrderStatusIterator : IIterator<Order>
    {
        private readonly List<Order> _orders;
        private readonly string _status;
        private int _position = -1;

        public OrderStatusIterator(List<Order> orders, string status)
        {
            _orders = orders;
            _status = status;
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
            // Duyệt từng phần tử cho đến khi tìm thấy order có status match
            while (true)
            {
                _position++;
                
                if (_position >= _orders.Count)
                    return false;

                // Nếu status match thì return true
                if (_orders[_position].Status == _status)
                    return true;
                
                // Nếu không match thì tiếp tục loop
            }
        }

        public void Reset()
        {
            _position = -1;
        }
    }
}
