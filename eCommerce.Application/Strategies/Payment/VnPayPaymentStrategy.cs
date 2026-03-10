using eCommerce.Core.Entities;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace eCommerce.Application.Strategies.Payment
{
    public class VnPayPaymentStrategy : IPaymentStrategy
    {
        private readonly IConfiguration _configuration;

        public VnPayPaymentStrategy(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ProviderName => "VNPAY";

        public Task<string> ExecutePaymentAsync(Order order)
        {
            // Lấy config từ appsettings.json
            var vnpUrl = _configuration["Vnpay:Url"];
            var hashSecret = _configuration["Vnpay:HashSecret"];
            var tmnCode = _configuration["Vnpay:TmnCode"];

            // Logic build URL của VNPAY mà em đã viết (đây là ví dụ tóm tắt)
            StringBuilder hashData = new StringBuilder();
            hashData.Append($"vnp_Amount={order.Total * 100}&");
            hashData.Append($"vnp_Command=pay&");
            hashData.Append($"vnp_CreateDate={order.CreatedAt:yyyyMMddHHmmss}&");
            hashData.Append($"vnp_CurrCode=VND&");
            hashData.Append($"vnp_OrderInfo=Thanh toan don hang {order.Id}&");
            hashData.Append($"vnp_TmnCode={tmnCode}&");
            hashData.Append($"vnp_TxnRef={order.Id}");

            string secureHash;
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(hashData.ToString()));
                secureHash = string.Concat(hashBytes.Select(b => b.ToString("x2")));
            }

            var redirectUrl = $"{vnpUrl}?{hashData.ToString()}&vnp_SecureHash={secureHash}";
            
            return Task.FromResult(redirectUrl);
        }
    }
}