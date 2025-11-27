using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface IUserService
    {
        Task<int> RegisterAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetCustomersCountAsync();
        Task<int> GetVendorsCountAsync();
    }
}