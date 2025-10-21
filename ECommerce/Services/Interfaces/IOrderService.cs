using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(Order order, IEnumerable<OrderItem> items, Payment payment, ShippingAddress address);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<Order?> GetByIdAsync(int id);
    }
}
