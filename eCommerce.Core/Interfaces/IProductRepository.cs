using eCommerce.Core.Entities;
namespace eCommerce.Core.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);

        Task<IEnumerable<Product>> GetFilteredAsync(
            int? categoryId,
            int? brandId, 
            string color, 
            string size, 
            decimal? minPrice, 
            decimal? maxPrice,
            string sortBy
        );

        Task<List<string>> GetUniqueColorsAsync();

        Task<List<string>> GetUniqueSizesAsync();
    }
}