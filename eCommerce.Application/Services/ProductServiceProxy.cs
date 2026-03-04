using eCommerce.Core.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace eCommerce.Application.Services
{
    public class ProductServiceProxy : IProductService
    {
        private readonly IProductService _realService;
        private readonly IMemoryCache _cache;
        private readonly string _allProductsCacheKey = "AllProducts_CacheKey";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10); // Lưu cache trong 10 phút

        public ProductServiceProxy(IProductService realService, IMemoryCache cache)
        {
            _realService = realService;
            _cache = cache;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            // Kiểm tra trong Cache trước
            if (!_cache.TryGetValue(_allProductsCacheKey, out IEnumerable<Product> products))
            {
                // Nếu không có, gọi Service thật
                products = await _realService.GetAllProductsAsync();

                // Lưu kết quả vào Cache
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(_cacheDuration);
                _cache.Set(_allProductsCacheKey, products, cacheOptions);
            }
            return products;
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            string cacheKey = $"Product_{id}";

            if (!_cache.TryGetValue(cacheKey, out Product? product))
            {
                product = await _realService.GetProductByIdAsync(id);
                if (product != null)
                {
                    _cache.Set(cacheKey, product, _cacheDuration);
                }
            }
            return product;
        }

        // Khi dữ liệu thay đổi (Thêm/Sửa/Xóa), ta phải xóa Cache cũ để tránh dữ liệu bị sai lệch
        public async Task CreateProductAsync(Product product)
        {
            await _realService.CreateProductAsync(product);
            _cache.Remove(_allProductsCacheKey); // Xóa cache danh sách
        }

        public async Task UpdateProductAsync(Product product)
        {
            await _realService.UpdateProductAsync(product);
            _cache.Remove(_allProductsCacheKey);
            _cache.Remove($"Product_{product.Id}");
        }

        public async Task DeleteProductAsync(int id)
        {
            await _realService.DeleteProductAsync(id);
            _cache.Remove(_allProductsCacheKey);
            _cache.Remove($"Product_{id}");
        }
    }
}