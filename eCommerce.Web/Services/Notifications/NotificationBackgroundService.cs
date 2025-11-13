using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using eCommerce.Web.Services.Notifications;

namespace eCommerce.Web.Services.Notifications
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly INotificationQueue _queue;
        private readonly IServiceProvider _services;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(INotificationQueue queue, IServiceProvider services, ILogger<NotificationBackgroundService> logger)
        {
            _queue = queue;
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var msg = await _queue.DequeueAsync(stoppingToken);

                    // create a scope so we can resolve scoped services like IPushService
                    using (var scope = _services.CreateScope())
                    {
                        var pushService = scope.ServiceProvider.GetService(typeof(IPushService)) as IPushService;
                        var emailSender = scope.ServiceProvider.GetService(typeof(Microsoft.AspNetCore.Identity.UI.Services.IEmailSender)) as Microsoft.AspNetCore.Identity.UI.Services.IEmailSender;

                        // send push
                        if (pushService != null)
                        {
                            try
                            {
                                await pushService.SendPushAsync(msg.UserId ?? string.Empty, msg.Title, msg.Body, new { url = msg.Url });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to send push for user {UserId}", msg.UserId);
                            }
                        }

                        // send email if present
                        if (!string.IsNullOrEmpty(msg.EmailTo) && !string.IsNullOrEmpty(msg.EmailSubject) && !string.IsNullOrEmpty(msg.EmailBody) && emailSender != null)
                        {
                            try
                            {
                                await emailSender.SendEmailAsync(msg.EmailTo, msg.EmailSubject, msg.EmailBody);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to send email to {Email}", msg.EmailTo);
                            }
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing notification queue");
                    // small delay to avoid tight loop on persistent failure
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }

            _logger.LogInformation("Notification background service stopping.");
        }
    }
}
