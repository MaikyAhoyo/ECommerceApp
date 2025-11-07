using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface IProductService
    {
        Task<int> CreateAsync(Product product);
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetTop4Async();
        Task<IEnumerable<Product>> GetByVendorIdAsync(int vendorId);
        Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Product>> SearchByNameAsync(string name);
        Task<IEnumerable<Product>> FilterByMetalAsync(string metal);
        Task<bool> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateStockAsync(int id, int quantity);
        Task<bool> AddCategoryToProductAsync(int productId, int categoryId);
        Task<bool> RemoveCategoryFromProductAsync(int productId, int categoryId);
        Task<IEnumerable<Category>> GetProductCategoriesAsync(int productId);
    }
}