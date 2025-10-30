namespace eCommerce.Web.Services
{
    // Simple in-memory voucher store
    public class VoucherService : IVoucherService
    {
        private readonly Dictionary<string, decimal> _vouchers = new(StringComparer.OrdinalIgnoreCase)
        {
            // code => percentage discount (0.10 = 10%) or fixed amount (we'll treat <1 as percent)
            { "WELCOME10", 0.10m },
            { "SHIPFREE", 0.00m },
            { "OFF50K", 50000m }
        };

        public Task<bool> ValidateVoucherAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return Task.FromResult(false);
            return Task.FromResult(_vouchers.ContainsKey(code));
        }

        public Task<decimal> GetDiscountAmountAsync(string code, decimal subTotal)
        {
            if (string.IsNullOrWhiteSpace(code)) return Task.FromResult(0m);
            if (!_vouchers.TryGetValue(code, out var value)) return Task.FromResult(0m);
            if (value > 0 && value < 1) // percentage
            {
                return Task.FromResult(Math.Round(subTotal * value, 0));
            }
            return Task.FromResult(value);
        }

        public Task<Dictionary<string, string>> GetAvailableVouchersAsync()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in _vouchers)
            {
                var display = kv.Value switch
                {
                    var v when v > 0 && v < 1 => $"{kv.Key} - {Math.Round(v * 100)}% off",
                    0m => $"{kv.Key} - Free shipping",
                    var v when v >= 1 => $"{kv.Key} - {v:N0} ₫ off",
                    _ => kv.Key
                };
                result[kv.Key] = display;
            }
            return Task.FromResult(result);
        }
    }
}
