using eCommerce.Core.Entities;

namespace eCommerce.Web.Services
{
    public interface ICartService
    {
        Task AddToCartAsync(int productId, int quantity);
        Task RemoveFromCartAsync(int productId);
        Task UpdateQuantityAsync(int productId, int quantity);
        Task<List<CartItem>> GetCartAsync();
        Task ClearCartAsync();
        Task ApplyVoucherAsync(string code);
    // Replace the cart contents entirely with the provided items (used for reorder)
    Task SetCartAsync(List<CartItem> cartItems);
        Task<decimal> CalculateShippingAsync(string country, string province);
        // Return voucher code currently applied or null
        Task<string?> GetAppliedVoucherAsync();
        // Persist shipping fee into session and retrieve it
        Task SetShippingAsync(decimal fee);
        Task<decimal?> GetShippingAsync();
    }
}
