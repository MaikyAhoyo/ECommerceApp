using Dapper;
using ECommerce.Data;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly DapperContext _context;

        public UserService(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> RegisterAsync(User user)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO Users (Name, Email, PasswordHash, Role, CreatedAt)
                        VALUES (@Name, @Email, @PasswordHash, @Role, @CreatedAt);
                        SELECT CAST(SCOPE_IDENTITY() as int)";
            return await connection.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Email = @Email";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Users";
            return await connection.QueryAsync<User>(sql);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var connection = _context.CreateConnection();
            var sql = @"UPDATE Users 
                        SET Name = @Name, 
                            Email = @Email, 
                            Role = @Role,
                            PasswordHash = @PasswordHash 
                        WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, user);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM Users WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT COUNT(*) FROM Users";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<int> GetCustomersCountAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT COUNT(*) FROM Users WHERE Role = 'Customer'";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<int> GetVendorsCountAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT COUNT(*) FROM Users WHERE Role = 'Vendor'";
            return await connection.ExecuteScalarAsync<int>(sql);
        }
    }
}