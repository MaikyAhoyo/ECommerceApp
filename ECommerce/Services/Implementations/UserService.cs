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
            var sql = "SELECT * FROM Users WHERE Email=@Email AND PasswordHash=@Password";
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
    }
}
