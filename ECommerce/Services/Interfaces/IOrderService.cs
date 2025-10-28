using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateAsync(Order order);
        Task<Order?> GetByIdAsync(int id);
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<Order?> GetActiveCartAsync(int userId);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> UpdateAsync(Order order);
        Task<bool> DeleteAsync(int id);
        Task<bool> AddItemToOrderAsync(int orderId, OrderItem item);
        Task<bool> RemoveItemFromOrderAsync(int orderItemId);
        Task<bool> UpdateOrderItemQuantityAsync(int orderItemId, int quantity);
        Task<IEnumerable<OrderItem>> GetOrderItemsAsync(int orderId);
        Task<decimal> CalculateOrderTotalAsync(int orderId);
        Task<bool> CheckoutOrderAsync(int orderId);
    }
}