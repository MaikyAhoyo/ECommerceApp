using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
    }
}
