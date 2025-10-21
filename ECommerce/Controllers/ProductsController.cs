using ECommerce.Models.ViewModels;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IReviewService _reviewService;

        public ProductsController(IProductService productService, ICategoryService categoryService, IReviewService reviewService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            var reviews = await _reviewService.GetProductReviewsAsync(id);
            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Reviews = reviews
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string keyword)
        {
            var products = await _productService.SearchAsync(keyword);
            return View(products);
        }
    }
}
