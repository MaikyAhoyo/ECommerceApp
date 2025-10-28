using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface IReviewService
    {
        Task<int> CreateAsync(Review review);
        Task<Review?> GetByIdAsync(int id);
        Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetByUserIdAsync(int userId);
        Task<bool> UpdateAsync(Review review);
        Task<bool> DeleteAsync(int id);
        Task<double> GetAverageRatingByProductAsync(int productId);
        Task<int> GetReviewCountByProductAsync(int productId);
    }
}