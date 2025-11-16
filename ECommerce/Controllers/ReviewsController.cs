using Microsoft.AspNetCore.Mvc;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authenticated user for creating reviews
    [IgnoreAntiforgeryToken]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        // DTO para recibir datos del cliente
        public class CreateReviewDto
        {
            public int ProductId { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; } = string.Empty;
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            try
            {
                // Log para debugging
                _logger.LogInformation($"Received review request: ProductId={dto.ProductId}, Rating={dto.Rating}");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning($"ModelState invalid: {string.Join(", ", errors)}");
                    return BadRequest(new { message = "Datos inválidos", errors });
                }

                // Validar rating
                if (dto.Rating < 1 || dto.Rating > 5)
                {
                    return BadRequest(new { message = "La calificación debe estar entre 1 y 5" });
                }

                // Validar comentario
                if (string.IsNullOrWhiteSpace(dto.Comment))
                {
                    return BadRequest(new { message = "El comentario es requerido" });
                }

                if (dto.Comment.Length > 500)
                {
                    return BadRequest(new { message = "El comentario no puede exceder 500 caracteres" });
                }

                // Obtener UserId del usuario autenticado
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("User not authenticated or invalid UserId claim");
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                // Crear el objeto Review
                var review = new Review
                {
                    ProductId = dto.ProductId,
                    UserId = userId,
                    Rating = dto.Rating,
                    Comment = dto.Comment.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                // Guardar en la base de datos
                var reviewId = await _reviewService.CreateAsync(review);
                var createdReview = await _reviewService.GetByIdAsync(reviewId);

                _logger.LogInformation($"Review created successfully with Id={reviewId}");

                return Ok(new
                {
                    success = true,
                    message = "Reseña creada exitosamente",
                    reviewId = reviewId,
                    review = createdReview
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, new { message = "Error al crear la reseña", detail = ex.Message });
            }
        }

        // GET: api/reviews/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var review = await _reviewService.GetByIdAsync(id);

                if (review == null)
                    return NotFound(new { message = "Reseña no encontrada" });

                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting review {id}");
                return StatusCode(500, new { message = "Error al obtener la reseña" });
            }
        }

        // GET: api/reviews/product/{productId}
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            try
            {
                var reviews = await _reviewService.GetByProductIdAsync(productId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting reviews for product {productId}");
                return StatusCode(500, new { message = "Error al obtener las reseñas" });
            }
        }

        // GET: api/reviews/user/{userId}
        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try
            {
                var reviews = await _reviewService.GetByUserIdAsync(userId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting reviews for user {userId}");
                return StatusCode(500, new { message = "Error al obtener las reseñas" });
            }
        }

        // GET: api/reviews/product/{productId}/average
        [HttpGet("product/{productId}/average")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAverageRating(int productId)
        {
            try
            {
                var average = await _reviewService.GetAverageRatingByProductAsync(productId);
                var count = await _reviewService.GetReviewCountByProductAsync(productId);

                return Ok(new { averageRating = average, reviewCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting average rating for product {productId}");
                return StatusCode(500, new { message = "Error al obtener el promedio" });
            }
        }

        // PUT: api/reviews/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateReviewDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (dto.Rating < 1 || dto.Rating > 5)
                    return BadRequest(new { message = "La calificación debe estar entre 1 y 5" });

                // Verificar que la reseña existe y pertenece al usuario
                var existingReview = await _reviewService.GetByIdAsync(id);
                if (existingReview == null)
                    return NotFound(new { message = "Reseña no encontrada" });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                if (existingReview.UserId != userId)
                {
                    return Forbid(); // Usuario no es el propietario de la reseña
                }

                var review = new Review
                {
                    Id = id,
                    ProductId = dto.ProductId,
                    UserId = userId,
                    Rating = dto.Rating,
                    Comment = dto.Comment.Trim(),
                    CreatedAt = existingReview.CreatedAt // Mantener fecha original
                };

                var success = await _reviewService.UpdateAsync(review);

                if (!success)
                    return NotFound(new { message = "Reseña no encontrada" });

                return Ok(new { success = true, message = "Reseña actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating review {id}");
                return StatusCode(500, new { message = "Error al actualizar la reseña" });
            }
        }

        // DELETE: api/reviews/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Verificar que la reseña existe y pertenece al usuario
                var existingReview = await _reviewService.GetByIdAsync(id);
                if (existingReview == null)
                    return NotFound(new { message = "Reseña no encontrada" });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                if (existingReview.UserId != userId)
                {
                    return Forbid(); // Usuario no es el propietario de la reseña
                }

                var success = await _reviewService.DeleteAsync(id);

                if (!success)
                    return NotFound(new { message = "Reseña no encontrada" });

                return Ok(new { success = true, message = "Reseña eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting review {id}");
                return StatusCode(500, new { message = "Error al eliminar la reseña" });
            }
        }
    }
}