using WebPush;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using eCommerce.Infrastructure.Data;
using eCommerce.Core.Entities;     

namespace eCommerce.Web.Services.Notifications
{
    public class WebPushService : IPushService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context; 
        public WebPushService(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task SendPushAsync(string userId, string title, string body, object data)
        {
            var subject = _configuration["Vapid:Subject"];
            var publicKey = _configuration["Vapid:PublicKey"];
            var privateKey = _configuration["Vapid:PrivateKey"];

            var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
            var webPushClient = new WebPushClient();

            
            
            var subscriptions = await _context.UserPushSubscriptions 
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = title,
                body = body,
                url = (data as dynamic)?.url 
            });

            foreach (var sub in subscriptions)
            {
                try
                {
                    var pushSubscription = new WebPush.PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                    await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
                }
                catch (WebPushException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.Gone)
                    {
                        _context.UserPushSubscriptions.Remove(sub);
                    }
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}