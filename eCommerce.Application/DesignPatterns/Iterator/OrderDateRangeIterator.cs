using eCommerce.Core.Entities;

namespace eCommerce.Application.DesignPatterns.Iterator
{
    /// <summary>
    /// Concrete Iterator - duyệt orders theo khoảng ngày
    /// Không tạo list mới, duyệt trực tiếp và skip phần tử không match
    /// </summary>
    public class OrderDateRangeIterator : IIterator<Order>
    {
        private readonly List<Order> _orders;
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;
        private int _position = -1;

        public OrderDateRangeIterator(List<Order> orders, DateTime fromDate, DateTime toDate)
        {
            _orders = orders;
            _fromDate = fromDate;
            _toDate = toDate;
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
            // Duyệt từng phần tử cho đến khi tìm thấy order trong khoảng ngày
            while (true)
            {
                _position++;
                
                if (_position >= _orders.Count)
                    return false;

                var orderDate = _orders[_position].CreatedAt;
                
                // Nếu date trong range thì return true
                if (orderDate >= _fromDate && orderDate <= _toDate)
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
