using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface IShippingAddressService
    {
        Task<int> CreateAsync(ShippingAddress address);
        Task<ShippingAddress?> GetByIdAsync(int id);
        Task<IEnumerable<ShippingAddress>> GetByUserIdAsync(int userId);
        Task<bool> UpdateAsync(ShippingAddress address);
        Task<bool> DeleteAsync(int id);
    }
}