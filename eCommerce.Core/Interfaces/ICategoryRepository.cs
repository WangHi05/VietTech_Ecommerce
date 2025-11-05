using eCommerce.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eCommerce.Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
    }
}