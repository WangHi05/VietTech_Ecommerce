using System.Threading.Tasks;

namespace eCommerce.Web.Services.Notifications
{
    public interface INotificationQueue
    {
        void Enqueue(NotificationMessage message);
        ValueTask<NotificationMessage> DequeueAsync(CancellationToken cancellationToken);
    }
}
