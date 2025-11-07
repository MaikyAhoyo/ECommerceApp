using Microsoft.AspNetCore.Mvc;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var productId = await _productService.CreateAsync(product);
            var createdProduct = await _productService.GetByIdAsync(productId);

            return CreatedAtAction(nameof(GetById), new { id = productId }, createdProduct);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(product);
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        // GET: api/products/vendor/{vendorId}
        [HttpGet("vendor/{vendorId}")]
        public async Task<IActionResult> GetByVendor(int vendorId)
        {
            var products = await _productService.GetByVendorIdAsync(vendorId);
            return Ok(products);
        }

        // GET: api/products/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _productService.GetByCategoryIdAsync(categoryId);
            return Ok(products);
        }

        // GET: api/products/top4
        [HttpGet("top4")]
        public async Task<IActionResult> GetTop4()
        {
            var products = await _productService.GetTop4Async();
            return Ok(products);
        }

        // GET: api/products/search?keyword=anillo
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return BadRequest(new { message = "Keyword is required" });

            var products = await _productService.SearchByNameAsync(keyword);
            return Ok(products);
        }

        // GET: api/products/metal/{metal}
        [HttpGet("metal/{metal}")]
        public async Task<IActionResult> FilterByMetal(string metal)
        {
            var products = await _productService.FilterByMetalAsync(metal);
            return Ok(products);
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            product.Id = id;
            var success = await _productService.UpdateAsync(product);

            if (!success)
                return NotFound(new { message = "Product not found" });

            return Ok(new { message = "Product updated successfully" });
        }

        // PATCH: api/products/{id}/stock
        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockRequest request)
        {
            var success = await _productService.UpdateStockAsync(id, request.Quantity);

            if (!success)
                return NotFound(new { message = "Product not found" });

            return Ok(new { message = "Stock updated successfully" });
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Product not found" });

            return Ok(new { message = "Product deleted successfully" });
        }

        // POST: api/products/{id}/categories/{categoryId}
        [HttpPost("{id}/categories/{categoryId}")]
        public async Task<IActionResult> AddCategory(int id, int categoryId)
        {
            var success = await _productService.AddCategoryToProductAsync(id, categoryId);

            if (!success)
                return BadRequest(new { message = "Failed to add category" });

            return Ok(new { message = "Category added successfully" });
        }

        // DELETE: api/products/{id}/categories/{categoryId}
        [HttpDelete("{id}/categories/{categoryId}")]
        public async Task<IActionResult> RemoveCategory(int id, int categoryId)
        {
            var success = await _productService.RemoveCategoryFromProductAsync(id, categoryId);

            if (!success)
                return BadRequest(new { message = "Failed to remove category" });

            return Ok(new { message = "Category removed successfully" });
        }

        // GET: api/products/{id}/categories
        [HttpGet("{id}/categories")]
        public async Task<IActionResult> GetProductCategories(int id)
        {
            var categories = await _productService.GetProductCategoriesAsync(id);
            return Ok(categories);
        }
    }

    public class UpdateStockRequest
    {
        public int Quantity { get; set; }
    }
}