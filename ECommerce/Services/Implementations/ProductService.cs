using Dapper;
using ECommerce.Data;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;
using System.Data;

namespace ECommerce.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly DapperContext _context;

        public ProductService(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(Product product)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO Products (Name, Description, Price, OriginalPrice, Discount, Stock, VendorId, ImageUrl, Metal, Purity)
                        VALUES (@Name, @Description, @Price, @OriginalPrice, @Discount, @Stock, @VendorId, @ImageUrl, @Metal, @Purity);
                        SELECT CAST(SCOPE_IDENTITY() as int)";
            return await connection.ExecuteScalarAsync<int>(sql, product);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Products WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Products";
            return await connection.QueryAsync<Product>(sql);
        }

        public async Task<IEnumerable<Product>> GetTop4Async()
        {
            using var connection = _context.CreateConnection();
            var procedureName = "GetTop4PopularProducts";

            var result = await connection.QueryAsync<Product>(
                procedureName,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<IEnumerable<Product>> GetByVendorIdAsync(int vendorId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Products WHERE VendorId = @VendorId";
            return await connection.QueryAsync<Product>(sql, new { VendorId = vendorId });
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            using var connection = _context.CreateConnection();
            var sql = @"SELECT p.* FROM Products p
                        INNER JOIN ProductCategories pc ON p.Id = pc.ProductId
                        WHERE pc.CategoryId = @CategoryId";
            return await connection.QueryAsync<Product>(sql, new { CategoryId = categoryId });
        }

        public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Products WHERE Name LIKE @Name OR Description LIKE @Name";
            return await connection.QueryAsync<Product>(sql, new { Name = $"%{name}%" });
        }

        public async Task<IEnumerable<Product>> FilterByMetalAsync(string metal)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Products WHERE Metal = @Metal";
            return await connection.QueryAsync<Product>(sql, new { Metal = metal });
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            using var connection = _context.CreateConnection();
            var sql = @"UPDATE Products 
                        SET Name = @Name, Description = @Description, Price = @Price, OriginalPrice = @OriginalPrice,
                            Discount = @Discount, Stock = @Stock, ImageUrl = @ImageUrl, Metal = @Metal, Purity = @Purity
                        WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, product);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM Products WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateStockAsync(int id, int quantityChange)
        {
            using var connection = _context.CreateConnection();
            var sql = "UPDATE Products SET Stock = Stock + @QuantityChange WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, QuantityChange = quantityChange });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateDiscountAsync(int id, decimal discount)
        {
            using var connection = _context.CreateConnection();

            var originalPrice = await connection.ExecuteScalarAsync<decimal>(
                "SELECT OriginalPrice FROM Products WHERE Id = @Id",
                new { Id = id }
            );

            if (originalPrice <= 0)
                return false;

            if (discount < 0) discount = 0;
            if (discount > 100) discount = 100;

            decimal discountedPrice = discount > 0
                ? originalPrice * (1 - (discount / 100m))
                : originalPrice;

            discountedPrice = RoundPriceToNearestEnding(discountedPrice);

            var sql = @"
                      UPDATE Products 
                      SET Discount = @Discount, 
                      Price = @NewPrice
                      WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                Discount = discount,
                NewPrice = discountedPrice
            });

            return rowsAffected > 0;
        }

        private decimal RoundPriceToNearestEnding(decimal price)
        {
            decimal whole = Math.Floor(price);
            decimal fraction = price - whole;

            decimal roundedFraction;

            if (fraction < 0.25m)
                roundedFraction = 0.00m;
            else if (fraction < 0.75m)
                roundedFraction = 0.50m;
            else
                roundedFraction = 0.99m;

            return whole + roundedFraction;
        }

        public async Task<bool> AddCategoryToProductAsync(int productId, int categoryId)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO ProductCategories (ProductId, CategoryId)
                        VALUES (@ProductId, @CategoryId)";
            var rowsAffected = await connection.ExecuteAsync(sql, new { ProductId = productId, CategoryId = categoryId });
            return rowsAffected > 0;
        }

        public async Task<bool> RemoveCategoryFromProductAsync(int productId, int categoryId)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM ProductCategories WHERE ProductId = @ProductId AND CategoryId = @CategoryId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { ProductId = productId, CategoryId = categoryId });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Category>> GetProductCategoriesAsync(int productId)
        {
            using var connection = _context.CreateConnection();
            var sql = @"SELECT c.* FROM Categories c
                        INNER JOIN ProductCategories pc ON c.Id = pc.CategoryId
                        WHERE pc.ProductId = @ProductId";
            return await connection.QueryAsync<Category>(sql, new { ProductId = productId });
        }
    }
}