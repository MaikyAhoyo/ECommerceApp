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

        public async Task<int> AddReviewAsync(Review review)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO Reviews (ProductId, UserId, Rating, Comment, CreatedAt)
                        VALUES (@ProductId, @UserId, @Rating, @Comment, @CreatedAt);
                        SELECT CAST(SCOPE_IDENTITY() as int)";
            return await connection.ExecuteScalarAsync<int>(sql, review);
        }

        public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM Reviews WHERE ProductId=@ProductId";
            return await connection.QueryAsync<Review>(sql, new { ProductId = productId });
        }
    }
}
