using Dapper;
using ECommerce.Data;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly DapperContext _context;

        public ReviewService(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(Review review)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO Reviews (ProductId, UserId, Rating, Comment, CreatedAt)
                        VALUES (@ProductId, @UserId, @Rating, @Comment, @CreatedAt);
                        SELECT CAST(SCOPE_IDENTITY() as int)";
            return await connection.ExecuteScalarAsync<int>(sql, review);
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Reviews WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Review>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
        {
            using var connection = _context.CreateConnection();
            var sql = @"SELECT r.*, u.Id, u.Name, u.Email
                        FROM Reviews r
                        INNER JOIN Users u ON r.UserId = u.Id
                        WHERE r.ProductId = @ProductId
                        ORDER BY r.CreatedAt DESC";

            var reviews = await connection.QueryAsync<Review, User, Review>(
                sql,
                (review, user) =>
                {
                    review.User = user;
                    return review;
                },
                new { ProductId = productId },
                splitOn: "Id"
            );

            return reviews;
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            var sql = @"SELECT r.*, p.Id, p.Name, p.ImageUrl
                        FROM Reviews r
                        INNER JOIN Products p ON r.ProductId = p.Id
                        WHERE r.UserId = @UserId
                        ORDER BY r.CreatedAt DESC";

            var reviews = await connection.QueryAsync<Review, Product, Review>(
                sql,
                (review, product) =>
                {
                    review.Product = product;
                    return review;
                },
                new { UserId = userId },
                splitOn: "Id"
            );

            return reviews;
        }

        public async Task<bool> UpdateAsync(Review review)
        {
            using var connection = _context.CreateConnection();
            var sql = @"UPDATE Reviews 
                        SET Rating = @Rating, Comment = @Comment
                        WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, review);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM Reviews WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<double> GetAverageRatingByProductAsync(int productId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT AVG(CAST(Rating AS FLOAT)) FROM Reviews WHERE ProductId = @ProductId";
            var average = await connection.ExecuteScalarAsync<double?>(sql, new { ProductId = productId });
            return average ?? 0;
        }

        public async Task<int> GetReviewCountByProductAsync(int productId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT COUNT(*) FROM Reviews WHERE ProductId = @ProductId";
            return await connection.ExecuteScalarAsync<int>(sql, new { ProductId = productId });
        }
    }
}