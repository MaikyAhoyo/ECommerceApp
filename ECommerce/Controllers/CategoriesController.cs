using Microsoft.AspNetCore.Mvc;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // POST: api/categories
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoryId = await _categoryService.CreateAsync(category);
            var createdCategory = await _categoryService.GetByIdAsync(categoryId);

            return CreatedAtAction(nameof(GetById), new { id = categoryId }, createdCategory);
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            return Ok(category);
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        // GET: api/categories/{id}/products
        [HttpGet("{id}/products")]
        public async Task<IActionResult> GetProducts(int id)
        {
            var products = await _categoryService.GetProductsByCategoryAsync(id);
            return Ok(products);
        }

        // PUT: api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            category.Id = id;
            var success = await _categoryService.UpdateAsync(category);

            if (!success)
                return NotFound(new { message = "Category not found" });

            return Ok(new { message = "Category updated successfully" });
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Category not found" });

            return Ok(new { message = "Category deleted successfully" });
        }
    }
}