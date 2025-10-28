using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<int> CreateAsync(Category category);
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<bool> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
    }
}