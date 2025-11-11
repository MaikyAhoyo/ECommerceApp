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

        public async Task<int> CreateAsync(Order order)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO Orders (UserId, OrderDate, ArrivalDate, Status, Total)
                        VALUES (@UserId, @OrderDate, @ArrivalDate, @Status, @Total);
                        SELECT CAST(SCOPE_IDENTITY() as int)";
            return await connection.ExecuteScalarAsync<int>(sql, order);
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Orders WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { Id = id });
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var sql = @"SELECT o.*, u.Id, u.Name, u.Email, u.Role
                        FROM Orders o
                        INNER JOIN Users u ON o.UserId = u.Id
                        WHERE o.Id = @Id";

            var orderDictionary = new Dictionary<int, Order>();

            var orders = await connection.QueryAsync<Order, User, Order>(
                sql,
                (order, user) =>
                {
                    if (!orderDictionary.TryGetValue(order.Id, out var orderEntry))
                    {
                        orderEntry = order;
                        orderEntry.User = user;
                        orderDictionary.Add(orderEntry.Id, orderEntry);
                    }
                    return orderEntry;
                },
                new { Id = id },
                splitOn: "Id"
            );

            var result = orders.FirstOrDefault();

            if (result != null)
            {
                // Get order items
                var itemsSql = @"SELECT oi.*, p.Id, p.Name, p.Price, p.ImageUrl
                                 FROM OrderItems oi
                                 INNER JOIN Products p ON oi.ProductId = p.Id
                                 WHERE oi.OrderId = @OrderId";

                var items = await connection.QueryAsync<OrderItem, Product, OrderItem>(
                    itemsSql,
                    (item, product) =>
                    {
                        item.Product = product;
                        return item;
                    },
                    new { OrderId = id },
                    splitOn: "Id"
                );

                result.OrderItems = items.ToList();

                // Get payment if exists
                var paymentSql = "SELECT * FROM Payments WHERE OrderId = @OrderId";
                result.Payment = await connection.QueryFirstOrDefaultAsync<Payment>(paymentSql, new { OrderId = id });
            }

            return result;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Orders";
            return await connection.QueryAsync<Order>(sql);
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC";
            return await connection.QueryAsync<Order>(sql, new { UserId = userId });
        }

        public async Task<Order?> GetActiveCartAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Orders WHERE UserId = @UserId AND Status = 'Cart'";
            return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                SELECT Id, UserId, Total, Status, OrderDate
                FROM Orders
                WHERE OrderDate BETWEEN @StartDate AND @EndDate
                ORDER BY OrderDate DESC;
            ";

            return await connection.QueryAsync<Order>(sql, new { StartDate = startDate, EndDate = endDate });
        }

        public async Task<IEnumerable<Order>> GetOrdersWithItemsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                      SELECT 
                      o.Id, o.UserId, o.Total, o.Status, o.OrderDate,
                      oi.Id, oi.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice,
                      p.Id, p.Name, p.Price, p.ImageUrl
                      FROM Orders o
                      INNER JOIN OrderItems oi ON o.Id = oi.OrderId
                      INNER JOIN Products p ON oi.ProductId = p.Id
                      WHERE o.OrderDate BETWEEN @StartDate AND @EndDate
                      ORDER BY o.OrderDate DESC;";

            var orderDictionary = new Dictionary<int, Order>();

            var orders = await connection.QueryAsync<Order, OrderItem, Product, Order>(
                sql,
                (order, orderItem, product) =>
                {
                    if (!orderDictionary.TryGetValue(order.Id, out var currentOrder))
                    {
                        currentOrder = order;
                        currentOrder.OrderItems = new List<OrderItem>();
                        orderDictionary.Add(order.Id, currentOrder);
                    }

                    orderItem.Product = product;
                    currentOrder.OrderItems.Add(orderItem);

                    return currentOrder;
                },
                new { StartDate = startDate, EndDate = endDate },
                splitOn: "Id,Id"
            );

            return orderDictionary.Values;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            using var connection = _context.CreateConnection();
            var sql = "UPDATE Orders SET Status = @Status WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Status = status });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateAsync(Order order)
        {
            using var connection = _context.CreateConnection();
            var sql = @"UPDATE Orders 
                        SET OrderDate = @OrderDate, ArrivalDate = @ArrivalDate, 
                            Status = @Status, Total = @Total
                        WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, order);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM Orders WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> AddItemToOrderAsync(int orderId, OrderItem item)
        {
            using var connection = _context.CreateConnection();

            // Check if item already exists
            var checkSql = "SELECT Id FROM OrderItems WHERE OrderId = @OrderId AND ProductId = @ProductId";
            var existingId = await connection.QueryFirstOrDefaultAsync<int?>(checkSql,
                new { OrderId = orderId, item.ProductId });

            if (existingId.HasValue)
            {
                // Update quantity
                var updateSql = "UPDATE OrderItems SET Quantity = Quantity + @Quantity WHERE Id = @Id";
                var updated = await connection.ExecuteAsync(updateSql,
                    new { Id = existingId.Value, item.Quantity });
                return updated > 0;
            }
            else
            {
                // Insert new item
                var insertSql = @"INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
                                  VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice)";
                item.OrderId = orderId;
                var inserted = await connection.ExecuteAsync(insertSql, item);
                return inserted > 0;
            }
        }

        public async Task<bool> RemoveItemFromOrderAsync(int orderItemId)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM OrderItems WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = orderItemId });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateOrderItemQuantityAsync(int orderItemId, int quantity)
        {
            using var connection = _context.CreateConnection();
            var sql = "UPDATE OrderItems SET Quantity = @Quantity WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = orderItemId, Quantity = quantity });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsAsync(int orderId)
        {
            using var connection = _context.CreateConnection();
            var sql = @"SELECT oi.*, p.Id, p.Name, p.Price, p.ImageUrl
                        FROM OrderItems oi
                        INNER JOIN Products p ON oi.ProductId = p.Id
                        WHERE oi.OrderId = @OrderId";

            var items = await connection.QueryAsync<OrderItem, Product, OrderItem>(
                sql,
                (item, product) =>
                {
                    item.Product = product;
                    return item;
                },
                new { OrderId = orderId },
                splitOn: "Id"
            );

            return items;
        }

        public async Task<decimal> CalculateOrderTotalAsync(int orderId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT SUM(Quantity * UnitPrice) FROM OrderItems WHERE OrderId = @OrderId";
            var total = await connection.ExecuteScalarAsync<decimal?>(sql, new { OrderId = orderId });
            return total ?? 0;
        }

        public async Task<bool> CheckoutOrderAsync(int orderId)
        {
            using var connection = _context.CreateConnection();

            // Calculate total
            var total = await CalculateOrderTotalAsync(orderId);

            // Update order status and total
            var sql = @"UPDATE Orders 
                        SET Status = 'Pending', Total = @Total, OrderDate = @OrderDate
                        WHERE Id = @Id AND Status = 'Cart'";

            var rowsAffected = await connection.ExecuteAsync(sql,
                new { Id = orderId, Total = total, OrderDate = DateTime.UtcNow });

            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Orders WHERE Status = @Status ORDER BY OrderDate DESC";
            return await connection.QueryAsync<Order>(sql, new { Status = status });
        }

        public async Task<int> GetTotalOrdersCountAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT COUNT(*) FROM Orders WHERE Status != 'Cart'";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = @"SELECT ISNULL(SUM(Total), 0) 
                        FROM Orders 
                        WHERE Status IN ('Confirmed', 'Shipped', 'Delivered')";
            return await connection.ExecuteScalarAsync<decimal>(sql);
        }
    }
}