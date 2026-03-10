using eCommerce.Core.Entities;
using System.Threading.Tasks;

namespace eCommerce.Core.Interfaces
{
    public interface IOrderObserver
    {
        // Nhận thông báo khi trạng thái thanh toán thay đổi
        Task OrderPaymentStatusChangedAsync(Order order, string paymentStatus);
        
        // Nhận thông báo khi trạng thái đơn hàng thay đổi
        Task OrderStatusChangedAsync(Order order, string status);
    }
}