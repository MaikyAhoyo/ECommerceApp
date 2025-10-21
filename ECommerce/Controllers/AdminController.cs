using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    public class AdminController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public AdminController(IProductService productService, IOrderService orderService, IUserService userService)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var products = await _productService.GetAllAsync();
            var orders = await _orderService.GetUserOrdersAsync(0); // Para demo
            var users = await _userService.GetByIdAsync(0); // Demo, podrías crear GetAll en IUserService
            return View();
        }

        public async Task<IActionResult> Products()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _orderService.GetUserOrdersAsync(0);
            return View(orders);
        }

        public async Task<IActionResult> Users()
        {
            // Aquí tendrías un método GetAllUsers() en UserService
            return View();
        }
    }
}
