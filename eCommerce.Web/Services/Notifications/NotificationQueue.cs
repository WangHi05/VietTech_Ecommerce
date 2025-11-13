using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Threading;

namespace eCommerce.Web.Services.Notifications
{
    public class NotificationQueue : INotificationQueue
    {
        private readonly Channel<NotificationMessage> _channel;

        public NotificationQueue()
        {
            // Unbounded channel is fine for small scale dev/test. For production consider bounded + backpressure.
            _channel = Channel.CreateUnbounded<NotificationMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public void Enqueue(NotificationMessage message)
        {
            if (!_channel.Writer.TryWrite(message))
            {
                // fallback - should not happen for unbounded
                _channel.Writer.WriteAsync(message);
            }
        }

        public async ValueTask<NotificationMessage> DequeueAsync(CancellationToken cancellationToken)
        {
            var item = await _channel.Reader.ReadAsync(cancellationToken);
            return item;
        }
    }
}
