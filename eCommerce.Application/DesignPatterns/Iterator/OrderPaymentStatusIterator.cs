using eCommerce.Core.Entities;

namespace eCommerce.Application.DesignPatterns.Iterator
{
    /// <summary>
    /// Concrete Iterator - duyệt orders theo payment status
    /// Không tạo list mới, duyệt trực tiếp và skip phần tử không match
    /// </summary>
    public class OrderPaymentStatusIterator : IIterator<Order>
    {
        private readonly List<Order> _orders;
        private readonly string _paymentStatus;
        private int _position = -1;

        public OrderPaymentStatusIterator(List<Order> orders, string paymentStatus)
        {
            _orders = orders;
            _paymentStatus = paymentStatus;
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
            // Duyệt từng phần tử cho đến khi tìm thấy order có payment status match
            while (true)
            {
                _position++;
                
                if (_position >= _orders.Count)
                    return false;

                // Nếu payment status match thì return true
                if (_orders[_position].PaymentStatus == _paymentStatus)
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
