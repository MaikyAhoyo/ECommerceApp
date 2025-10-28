using Dapper;
using ECommerce.Data;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly DapperContext _context;

        public CategoryService(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(Category category)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO Categories (Name, Description)
                        VALUES (@Name, @Description);
                        SELECT CAST(SCOPE_IDENTITY() as int)";
            return await connection.ExecuteScalarAsync<int>(sql, category);
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Categories WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Categories";
            return await connection.QueryAsync<Category>(sql);
        }

        public async Task<bool> UpdateAsync(Category category)
        {
            using var connection = _context.CreateConnection();
            var sql = @"UPDATE Categories 
                        SET Name = @Name, Description = @Description
                        WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, category);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM Categories WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            using var connection = _context.CreateConnection();
            var sql = @"SELECT p.* FROM Products p
                        INNER JOIN ProductCategories pc ON p.Id = pc.ProductId
                        WHERE pc.CategoryId = @CategoryId";
            return await connection.QueryAsync<Product>(sql, new { CategoryId = categoryId });
        }
    }
}