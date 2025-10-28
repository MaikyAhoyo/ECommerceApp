using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<int> RegisterAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> ChangePasswordAsync(int id, string currentPassword, string newPassword);
        Task<IEnumerable<User>> GetVendorsAsync();
    }
}