using ECommerce.Models.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface IReviewService
    {
        Task<int> AddReviewAsync(Review review);
        Task<IEnumerable<Review>> GetProductReviewsAsync(int productId);
    }
}
