using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace eCommerce.Infrastructure.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category) // Tải kèm để hiển thị
                .Include(p => p.Brand)    // Tải kèm để hiển thị
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Product>> GetFilteredAsync(int? categoryId, int? brandId, string color, string size, decimal? minPrice, decimal? maxPrice, string sortBy)
        {
            // Bắt đầu với một IQueryable cơ sở
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable(); // .AsQueryable() rất quan trọng

            // Áp dụng các bộ lọc nếu chúng tồn tại
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (brandId.HasValue && brandId > 0)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            if (!string.IsNullOrEmpty(color))
            {
                query = query.Where(p => p.Color == color);
            }

            if (!string.IsNullOrEmpty(size))
            {
                query = query.Where(p => p.Size == size);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            switch (sortBy)
            {
                case "price-asc": // Giá tăng dần
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price-desc": // Giá giảm dần
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "name-asc": // Tên A-Z
                    query = query.OrderBy(p => p.Name);
                    break;
                case "name-desc": // Tên Z-A
                    query = query.OrderByDescending(p => p.Name);
                    break;
                default:
                    // Mặc định (ví dụ: sản phẩm mới nhất, hoặc theo tên)
                    query = query.OrderBy(p => p.Id); 
                    break;
            }
            // Cuối cùng, thực thi truy vấn
            return await query.AsNoTracking().ToListAsync();
        }

        // === LẤY CÁC GIÁ TRỊ DUY NHẤT ===
        public async Task<List<string>> GetUniqueColorsAsync()
        {
            return await _context.Products
                .Select(p => p.Color)
                .Where(c => !string.IsNullOrEmpty(c)) // Bỏ qua các màu rỗng
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetUniqueSizesAsync()
        {
            return await _context.Products
                .Select(p => p.Size)
                .Where(s => !string.IsNullOrEmpty(s)) // Bỏ qua các size rỗng
                .Distinct()
                .ToListAsync();
        }

    }
}
