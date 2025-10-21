using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<int> RegisterAsync(User user);
        Task<User?> GetByIdAsync(int id);
    }
}
