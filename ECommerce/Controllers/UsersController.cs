using ECommerce.Models.Entities;
using ECommerce.Models.ViewModels;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = model.Password,
                Role = "Customer",
                CreatedAt = DateTime.Now
            };

            await _userService.RegisterAsync(user);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userService.LoginAsync(email, password);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrecta");
                return View();
            }

            // Aquí podrías establecer sesión o cookie
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }
    }
}
