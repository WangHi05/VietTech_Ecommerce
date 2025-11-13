using eCommerce.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using eCommerce.Core.Entities;
using System.Linq;
using WebPush;

namespace eCommerce.Web.Services
{
    public class WebPushService : IPushService
    {
        private readonly AppDbContext _db;
        private readonly VapidDetails _vapid;

        public WebPushService(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            var publicKey = configuration["Vapid:PublicKey"] ?? string.Empty;
            var privateKey = configuration["Vapid:PrivateKey"] ?? string.Empty;
            var subject = configuration["Vapid:Subject"] ?? "mailto:noreply@example.com";
            _vapid = new VapidDetails(subject, publicKey, privateKey);
        }

        public async Task SendPushAsync(string userId, string title, string body, object? data = null)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var subs = await _db.PushSubscriptions.Where(s => s.UserId == userId).ToListAsync();
            if (subs == null || subs.Count == 0) return;

            var client = new WebPushClient();
            if (!string.IsNullOrEmpty(_vapid.PublicKey) && !string.IsNullOrEmpty(_vapid.PrivateKey))
            {
                try
                {
                    client.SetVapidDetails(_vapid.Subject, _vapid.PublicKey, _vapid.PrivateKey);
                }
                catch { /* swallow */ }
            }

            var payloadObj = new
            {
                title,
                body,
                data
            };
            var payload = JsonSerializer.Serialize(payloadObj);

            foreach (var s in subs)
            {
                try
                {
                    var pushSub = new WebPush.PushSubscription(s.Endpoint, s.P256DH, s.Auth);
                    await client.SendNotificationAsync(pushSub, payload);
                }
                catch (WebPushException ex)
                {
                    // If subscription is gone/invalid remove it
                    if (ex.StatusCode == System.Net.HttpStatusCode.Gone || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        try
                        {
                            _db.PushSubscriptions.Remove(s);
                            await _db.SaveChangesAsync();
                        }
                        catch { }
                    }
                    // else ignore
                }
                catch { }
            }
        }
    }
}
