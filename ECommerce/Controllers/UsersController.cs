using Microsoft.AspNetCore.Mvc;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            user.CreatedAt = DateTime.UtcNow;
            var userId = await _userService.RegisterAsync(user);

            if (userId == 0)
                return BadRequest(new { message = "Email already exists" });

            var createdUser = await _userService.GetByIdAsync(userId);
            return CreatedAtAction(nameof(GetById), new { id = userId }, createdUser);
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required" });

            var user = await _userService.LoginAsync(request.Email, request.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(user);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // GET: api/users/vendors
        [HttpGet("vendors")]
        public async Task<IActionResult> GetVendors()
        {
            var vendors = await _userService.GetVendorsAsync();
            return Ok(vendors);
        }

        // GET: api/users/email/{email}
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userService.GetByEmailAsync(email);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            user.Id = id;
            var success = await _userService.UpdateAsync(user);

            if (!success)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User updated successfully" });
        }

        // PUT: api/users/{id}/change-password
        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest(new { message = "Current and new password are required" });

            var success = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);

            if (!success)
                return BadRequest(new { message = "Current password is incorrect or user not found" });

            return Ok(new { message = "Password changed successfully" });
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _userService.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User deleted successfully" });
        }
    }

    // Request models
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}