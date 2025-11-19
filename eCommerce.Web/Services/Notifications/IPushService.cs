using System.Threading.Tasks;

namespace eCommerce.Web.Services.Notifications
{
    public interface IPushService
    {
        Task SendPushAsync(string userId, string title, string body, object data);
    }
}