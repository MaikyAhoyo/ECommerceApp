using Microsoft.AspNetCore.Mvc;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Review review)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (review.Rating < 1 || review.Rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5" });

            review.CreatedAt = DateTime.UtcNow;
            var reviewId = await _reviewService.CreateAsync(review);
            var createdReview = await _reviewService.GetByIdAsync(reviewId);

            return CreatedAtAction(nameof(GetById), new { id = reviewId }, createdReview);
        }

        // GET: api/reviews/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);

            if (review == null)
                return NotFound(new { message = "Review not found" });

            return Ok(review);
        }

        // GET: api/reviews/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var reviews = await _reviewService.GetByProductIdAsync(productId);
            return Ok(reviews);
        }

        // GET: api/reviews/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var reviews = await _reviewService.GetByUserIdAsync(userId);
            return Ok(reviews);
        }

        // GET: api/reviews/product/{productId}/average
        [HttpGet("product/{productId}/average")]
        public async Task<IActionResult> GetAverageRating(int productId)
        {
            var average = await _reviewService.GetAverageRatingByProductAsync(productId);
            var count = await _reviewService.GetReviewCountByProductAsync(productId);

            return Ok(new { averageRating = average, reviewCount = count });
        }

        // PUT: api/reviews/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Review review)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (review.Rating < 1 || review.Rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5" });

            review.Id = id;
            var success = await _reviewService.UpdateAsync(review);

            if (!success)
                return NotFound(new { message = "Review not found" });

            return Ok(new { message = "Review updated successfully" });
        }

        // DELETE: api/reviews/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _reviewService.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Review not found" });

            return Ok(new { message = "Review deleted successfully" });
        }
    }
}