using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;
using ECommerce.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /auth/login
        [HttpGet("login")]
        public IActionResult Login(string? returnUrl = null)
        {
            // Si ya está autenticado, redirigir según su rol
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleDashboard();
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        // POST: /auth/login
        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.LoginAsync(model.Email, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Email o contraseña incorrectos");
                return View(model);
            }

            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            TempData["Success"] = $"¡Bienvenido {user.Name}!";

            // Redirigir según returnUrl o rol
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return user.Role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Vendor" => RedirectToAction("Dashboard", "Vendor"),
                "Customer" => RedirectToAction("Home", "Customer"),
                _ => RedirectToAction("Login")
            };
        }

        // GET: /auth/register
        [HttpGet("register")]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleDashboard();
            }

            return View();
        }

        // POST: /auth/register
        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Verificar si el email ya existe
            var existingUser = await _userService.GetByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Este email ya está registrado");
                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = model.Password, // TODO: Hashear la contraseña con BCrypt
                Role = model.RegisterAsVendor ? "Vendor" : "Customer",
                CreatedAt = DateTime.UtcNow
            };

            var userId = await _userService.RegisterAsync(user);

            if (userId > 0)
            {
                TempData["Success"] = "Registro exitoso. Por favor inicia sesión.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError("", "Error al registrar el usuario");
            return View(model);
        }

        // POST: /auth/logout
        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Sesión cerrada exitosamente";
            return RedirectToAction(nameof(Login));
        }

        // GET: /auth/access-denied
        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper method
        private IActionResult RedirectToRoleDashboard()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Vendor" => RedirectToAction("Dashboard", "Vendor"),
                "Customer" => RedirectToAction("Home", "Customer"),
                _ => RedirectToAction(nameof(Login))
            };
        }
    }
}