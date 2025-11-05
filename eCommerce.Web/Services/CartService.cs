using eCommerce.Core.Entities;
using eCommerce.Application.Services;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace eCommerce.Web.Services
{
    // Simple session-backed cart service. Stores cart in session as JSON.
    public class CartService : ICartService
    {
        private const string SessionKey = "CartSession";
        private const string VoucherKey = "CartVoucher";
    private const string ShippingKey = "CartShipping";
        private const string CookieKey = "CartCookie";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;

        public CartService(IHttpContextAccessor httpContextAccessor, IProductService productService)
        {
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public async Task AddToCartAsync(int productId, int quantity)
        {
            var cart = await GetCartAsync();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item == null)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null) return;
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = Math.Max(1, quantity),
                    ImageUrl = product.ImageUrl
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            SaveCart(cart);
        }

        public async Task RemoveFromCartAsync(int productId)
        {
            var cart = await GetCartAsync();
            cart.RemoveAll(c => c.ProductId == productId);
            SaveCart(cart);
        }

        public async Task UpdateQuantityAsync(int productId, int quantity)
        {
            var cart = await GetCartAsync();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                item.Quantity = Math.Max(0, quantity);
                if (item.Quantity == 0) cart.Remove(item);
                SaveCart(cart);
            }
        }

        public Task<List<CartItem>> GetCartAsync()
        {
            var data = Session.GetString(SessionKey);
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<CartItem>>(data) ?? new List<CartItem>();

                    // Refresh ImageUrl for items in session in case product images changed on disk/DB
                    if (list.Any())
                    {
                        foreach (var ci in list)
                        {
                            try
                            {
                                var prod = _productService.GetProductByIdAsync(ci.ProductId).GetAwaiter().GetResult();
                                if (prod != null && !string.IsNullOrEmpty(prod.ImageUrl)) ci.ImageUrl = prod.ImageUrl;
                            }
                            catch { }
                        }
                        // persist refreshed list back to session/cookie
                        try { Session.SetString(SessionKey, JsonSerializer.Serialize(list)); } catch { }
                    }

                    return Task.FromResult(list);
                }
                catch
                {
                    return Task.FromResult(new List<CartItem>());
                }
            }

            // If session is empty, try to load from cookie (persisted cart)
            var req = _httpContextAccessor.HttpContext?.Request;
            if (req != null && req.Cookies.TryGetValue(CookieKey, out var cookie))
            {
                try
                {
                    using var doc = JsonDocument.Parse(cookie);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("items", out var itemsElem) && itemsElem.ValueKind == JsonValueKind.Array)
                    {
                        var list = new List<CartItem>();
                        foreach (var el in itemsElem.EnumerateArray())
                        {
                            try
                            {
                                var ci = JsonSerializer.Deserialize<CartItem>(el.GetRawText());
                                if (ci != null) list.Add(ci);
                            }
                            catch { }
                        }

                        // Refresh ImageUrl for items loaded from cookie
                        if (list.Any())
                        {
                            foreach (var ci in list)
                            {
                                try
                                {
                                    var prod = _productService.GetProductByIdAsync(ci.ProductId).GetAwaiter().GetResult();
                                    if (prod != null && !string.IsNullOrEmpty(prod.ImageUrl)) ci.ImageUrl = prod.ImageUrl;
                                }
                                catch { }
                            }
                        }

                        // restore voucher and shipping into session if present
                        if (root.TryGetProperty("voucher", out var v) && v.ValueKind == JsonValueKind.String)
                        {
                            var code = v.GetString() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(code)) Session.SetString(VoucherKey, code);
                        }
                        if (root.TryGetProperty("shipping", out var s) && s.ValueKind == JsonValueKind.String)
                        {
                            var sVal = s.GetString();
                            if (!string.IsNullOrEmpty(sVal)) Session.SetString(ShippingKey, sVal);
                        }

                        // populate session with cart json for continuity
                        try { Session.SetString(SessionKey, JsonSerializer.Serialize(list)); } catch { }
                        return Task.FromResult(list);
                    }
                }
                catch
                {
                    // ignore cookie parse errors and return empty cart
                }
            }

            return Task.FromResult(new List<CartItem>());
        }

        public Task ClearCartAsync()
        {
            Session.Remove(SessionKey);
            Session.Remove(VoucherKey);
            Session.Remove(ShippingKey);
            try
            {
                var resp = _httpContextAccessor.HttpContext?.Response;
                resp?.Cookies.Delete(CookieKey);
            }
            catch { }
            return Task.CompletedTask;
        }

        public Task SetCartAsync(List<CartItem> cartItems)
        {
            if (cartItems == null) cartItems = new List<CartItem>();
            // overwrite session and persisted cookie
            try
            {
                var json = JsonSerializer.Serialize(cartItems);
                Session.SetString(SessionKey, json);
                // update cookie via SaveCart behavior by calling SaveCart
                SaveCart(cartItems);
            }
            catch
            {
                // swallow errors - non-blocking
            }

            return Task.CompletedTask;
        }

        public Task ApplyVoucherAsync(string code)
        {
            // For now simply save voucher code in session. VoucherService will validate later.
            Session.SetString(VoucherKey, code ?? string.Empty);
            // update persisted cookie
            var cart = GetCartAsync().GetAwaiter().GetResult();
            SaveCart(cart);
            return Task.CompletedTask;
        }

        public Task<string?> GetAppliedVoucherAsync()
        {
            var code = Session.GetString(VoucherKey);
            if (string.IsNullOrWhiteSpace(code)) return Task.FromResult<string?>(null);
            return Task.FromResult<string?>(code);
        }

        public Task SetShippingAsync(decimal fee)
        {
            Session.SetString(ShippingKey, fee.ToString());
            var cart = GetCartAsync().GetAwaiter().GetResult();
            SaveCart(cart);
            return Task.CompletedTask;
        }

        public Task<decimal?> GetShippingAsync()
        {
            var s = Session.GetString(ShippingKey);
            if (string.IsNullOrEmpty(s)) return Task.FromResult<decimal?>(null);
            if (decimal.TryParse(s, out var v)) return Task.FromResult<decimal?>(v);
            return Task.FromResult<decimal?>(null);
        }

        public Task<decimal> CalculateShippingAsync(string country, string province)
        {
            // Preset shipping fees for a few provinces in Vietnam; otherwise default fees.
            if (string.IsNullOrEmpty(country)) country = string.Empty;
            if (string.IsNullOrEmpty(province)) province = string.Empty;

            if (string.Equals(country, "Vietnam", StringComparison.OrdinalIgnoreCase) || string.Equals(country, "VN", StringComparison.OrdinalIgnoreCase))
            {
                // Map known provinces
                var p = province.Trim();
                if (p.Equals("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) || p.Equals("Ho Chi Minh", StringComparison.OrdinalIgnoreCase) || p.Equals("HCM", StringComparison.OrdinalIgnoreCase))
                    return Task.FromResult(20000m);
                if (p.Equals("Hà Nội", StringComparison.OrdinalIgnoreCase) || p.Equals("Ha Noi", StringComparison.OrdinalIgnoreCase) || p.Equals("Hanoi", StringComparison.OrdinalIgnoreCase))
                    return Task.FromResult(20000m);
                if (p.Equals("Đà Nẵng", StringComparison.OrdinalIgnoreCase) || p.Equals("Da Nang", StringComparison.OrdinalIgnoreCase))
                    return Task.FromResult(30000m);

                // default in-country
                return Task.FromResult(35000m);
            }

            // international default
            return Task.FromResult(100000m);
        }

        private void SaveCart(List<CartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            Session.SetString(SessionKey, json);

            // Build envelope with voucher and shipping
            string? voucher = Session.GetString(VoucherKey);
            string? shipping = Session.GetString(ShippingKey);

            var envelope = new
            {
                items = cart,
                voucher = voucher ?? string.Empty,
                shipping = shipping ?? string.Empty
            };

            try
            {
                var cookieJson = JsonSerializer.Serialize(envelope);
                var resp = _httpContextAccessor.HttpContext?.Response;
                if (resp != null)
                {
                    var options = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(30),
                        HttpOnly = false,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax
                    };
                    resp.Cookies.Append(CookieKey, cookieJson, options);
                }
            }
            catch
            {
                // cookie write failure should not break flow
            }
        }
    }
}
