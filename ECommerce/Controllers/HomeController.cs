using Microsoft.AspNetCore.Mvc;
using ECommerce.Services.Interfaces;

namespace ECommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public HomeController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories;
            return View(products);
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
