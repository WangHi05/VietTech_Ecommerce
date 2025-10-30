namespace eCommerce.Web.Services
{
    public interface IVoucherService
    {
        Task<bool> ValidateVoucherAsync(string code);
        Task<decimal> GetDiscountAmountAsync(string code, decimal subTotal);
        // Return available vouchers: key=code, value=display text
        Task<Dictionary<string, string>> GetAvailableVouchersAsync();
    }
}
