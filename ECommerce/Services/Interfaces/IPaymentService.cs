using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<int> CreateAsync(Payment payment);
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> UpdateAsync(Payment payment);
        Task<bool> DeleteAsync(int id);
        Task<bool> ProcessPaymentAsync(int orderId, string method, decimal amount);
    }
}