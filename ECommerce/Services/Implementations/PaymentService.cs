using Dapper;
using ECommerce.Data;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly DapperContext _context;

        public PaymentService(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(Payment payment)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO Payments (OrderId, PaymentDate, Amount, Method, Status)
                        VALUES (@OrderId, @PaymentDate, @Amount, @Method, @Status);
                        SELECT CAST(SCOPE_IDENTITY() as int)";
            return await connection.ExecuteScalarAsync<int>(sql, payment);
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Payments WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { Id = id });
        }

        public async Task<Payment?> GetByOrderIdAsync(int orderId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Payments WHERE OrderId = @OrderId";
            return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { OrderId = orderId });
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Payments";
            return await connection.QueryAsync<Payment>(sql);
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            using var connection = _context.CreateConnection();
            var sql = "UPDATE Payments SET Status = @Status WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Status = status });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateAsync(Payment payment)
        {
            using var connection = _context.CreateConnection();
            var sql = @"UPDATE Payments 
                        SET PaymentDate = @PaymentDate, Amount = @Amount, 
                            Method = @Method, Status = @Status
                        WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, payment);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM Payments WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ProcessPaymentAsync(int orderId, string method, decimal amount)
        {
            using var connection = _context.CreateConnection();

            // Check if payment already exists
            var checkSql = "SELECT Id FROM Payments WHERE OrderId = @OrderId";
            var existingId = await connection.QueryFirstOrDefaultAsync<int?>(checkSql, new { OrderId = orderId });

            if (existingId.HasValue)
            {
                // Update existing payment
                var updateSql = @"UPDATE Payments 
                                  SET Method = @Method, Amount = @Amount, 
                                      Status = 'Completed', PaymentDate = @PaymentDate
                                  WHERE Id = @Id";
                var updated = await connection.ExecuteAsync(updateSql,
                    new { Id = existingId.Value, Method = method, Amount = amount, PaymentDate = DateTime.UtcNow });
                return updated > 0;
            }
            else
            {
                // Create new payment
                var insertSql = @"INSERT INTO Payments (OrderId, PaymentDate, Amount, Method, Status)
                                  VALUES (@OrderId, @PaymentDate, @Amount, @Method, 'Completed')";
                var inserted = await connection.ExecuteAsync(insertSql,
                    new { OrderId = orderId, PaymentDate = DateTime.UtcNow, Amount = amount, Method = method });

                if (inserted > 0)
                {
                    // Update order status
                    var orderUpdateSql = "UPDATE Orders SET Status = 'Confirmed' WHERE Id = @OrderId";
                    await connection.ExecuteAsync(orderUpdateSql, new { OrderId = orderId });
                    return true;
                }

                return false;
            }
        }
    }
}