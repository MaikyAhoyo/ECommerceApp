using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(Review review)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", "Products", new { id = review.ProductId });

            review.CreatedAt = DateTime.Now;
            await _reviewService.AddReviewAsync(review);
            return RedirectToAction("Details", "Products", new { id = review.ProductId });
        }
    }
}
