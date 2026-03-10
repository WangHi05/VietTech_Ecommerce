using eCommerce.Core.Entities;
using System.Threading.Tasks;

namespace eCommerce.Core.Interfaces
{
    public interface IOrderSubject
    {
        void Attach(IOrderObserver observer);
        void Detach(IOrderObserver observer);
        Task NotifyPaymentStatusChangedAsync(Order order, string paymentStatus);
        Task NotifyStatusChangedAsync(Order order, string status);
    }
}