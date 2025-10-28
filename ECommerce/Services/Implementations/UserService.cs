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

        public async Task<User?> LoginAsync(string email, string password)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Email = @Email AND PasswordHash = @Password";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email, Password = password });
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
                        SET Name = @Name, Email = @Email, Role = @Role
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

        public async Task<bool> ChangePasswordAsync(int id, string currentPassword, string newPassword)
        {
            using var connection = _context.CreateConnection();

            // Verify current password
            var verifySql = "SELECT COUNT(1) FROM Users WHERE Id = @Id AND PasswordHash = @CurrentPassword";
            var isValid = await connection.ExecuteScalarAsync<int>(verifySql,
                new { Id = id, CurrentPassword = currentPassword });

            if (isValid == 0)
                return false;

            // Update password
            var updateSql = "UPDATE Users SET PasswordHash = @NewPassword WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(updateSql,
                new { Id = id, NewPassword = newPassword });

            return rowsAffected > 0;
        }

        public async Task<IEnumerable<User>> GetVendorsAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Role = 'Vendor'";
            return await connection.QueryAsync<User>(sql);
        }
    }
}