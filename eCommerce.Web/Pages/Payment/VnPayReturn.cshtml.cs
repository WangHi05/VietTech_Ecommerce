using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Web.Pages.Payment
{
    public class VnPayReturnModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public VnPayReturnModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IsValidHash { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public void OnGet()
        {
            var query = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());

            // Extract secure hash from query
            query.TryGetValue("vnp_SecureHash", out var secureHash);
            query.TryGetValue("vnp_ResponseCode", out var resp);
            query.TryGetValue("vnp_TxnRef", out var txnRef);
            query.TryGetValue("vnp_Message", out var msg);

            ResponseCode = resp ?? string.Empty;
            OrderId = txnRef ?? string.Empty;
            Message = msg ?? string.Empty;

            // remove secure hash params for verification
            var filtered = query.Where(kv => kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                                .ToList();

            var data = string.Join('&', filtered.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

            var hashSecret = _configuration.GetValue<string>("VnPay:HashSecret");
            if (string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(secureHash))
            {
                IsValidHash = false;
                return;
            }

            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret)))
            {
                var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                var computedHex = BitConverter.ToString(computed).Replace("-", string.Empty).ToUpperInvariant();
                IsValidHash = string.Equals(computedHex, secureHash, StringComparison.OrdinalIgnoreCase);
            }

            // TODO: if IsValidHash and ResponseCode == "00" then mark order paid in DB via IOrderService
        }
    }
}
