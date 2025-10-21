using Dapper;
using ECommerce.Data;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly DapperContext _context;

        public OrderService(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> CreateOrderAsync(Order order, IEnumerable<OrderItem> items, Payment payment, ShippingAddress address)
        {
            using var connection = _context.CreateConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                var orderSql = @"INSERT INTO Orders (UserId, OrderDate, Status, Total)
                                 VALUES (@UserId, @OrderDate, @Status, @Total);
                                 SELECT CAST(SCOPE_IDENTITY() as int)";
                int orderId = await connection.ExecuteScalarAsync<int>(orderSql, order, transaction);

                foreach (var item in items)
                {
                    item.OrderId = orderId;
                    await connection.ExecuteAsync(@"INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
                                                    VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice)", item, transaction);
                }

                payment.OrderId = orderId;
                await connection.ExecuteAsync(@"INSERT INTO Payments (OrderId, PaymentDate, Amount, Method, Status)
                                                VALUES (@OrderId, @PaymentDate, @Amount, @Method, @Status)", payment, transaction);

                address.UserId = order.UserId;
                await connection.ExecuteAsync(@"INSERT INTO ShippingAddresses (UserId, AddressLine1, AddressLine2, City, State, Country, PostalCode)
                                                VALUES (@UserId, @AddressLine1, @AddressLine2, @City, @State, @Country, @PostalCode)", address, transaction);

                transaction.Commit();
                return orderId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Orders WHERE UserId=@UserId";
            return await connection.QueryAsync<Order>(sql, new { UserId = userId });
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Orders WHERE Id=@Id";
            return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { Id = id });
        }
    }
}
