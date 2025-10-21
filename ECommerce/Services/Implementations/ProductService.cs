using Dapper;
using ECommerce.Data;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly DapperContext _context;

        public ProductService(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Product>("SELECT * FROM Products");
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Product>(
                "SELECT * FROM Products WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Product>> SearchAsync(string keyword)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Product>(
                "SELECT * FROM Products WHERE Name LIKE @Keyword", new { Keyword = $"%{keyword}%" });
        }

        public async Task<int> CreateAsync(Product product)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO Products (Name, Description, Price, Stock, CategoryId, VendorId, ImageUrl)
                        VALUES (@Name, @Description, @Price, @Stock, @CategoryId, @VendorId, @ImageUrl);
                        SELECT CAST(SCOPE_IDENTITY() as int)";
            return await connection.ExecuteScalarAsync<int>(sql, product);
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            using var connection = _context.CreateConnection();
            var sql = @"UPDATE Products SET Name=@Name, Description=@Description, Price=@Price, Stock=@Stock,
                        CategoryId=@CategoryId, ImageUrl=@ImageUrl WHERE Id=@Id";
            return await connection.ExecuteAsync(sql, product) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM Products WHERE Id = @Id";
            return await connection.ExecuteAsync(sql, new { Id = id }) > 0;
        }
    }
}
