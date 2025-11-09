using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;
using ECommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;
        private readonly IReviewService _reviewService;
        private readonly IPaymentService _paymentService;

        public AdminController(
            IProductService productService,
            IOrderService orderService,
            IUserService userService,
            ICategoryService categoryService,
            IReviewService reviewService,
            IPaymentService paymentService)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
            _categoryService = categoryService;
            _reviewService = reviewService;
            _paymentService = paymentService;
        }

        #region Dashboard

        // GET: /admin/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalProducts = (await _productService.GetAllAsync()).Count(),
                TotalOrders = await _orderService.GetTotalOrdersCountAsync(),
                TotalRevenue = await _orderService.GetTotalRevenueAsync(),
                TotalUsers = await _userService.GetTotalUsersCountAsync(),
                TotalCustomers = await _userService.GetCustomersCountAsync(),
                TotalVendors = await _userService.GetVendorsCountAsync(),
                RecentOrders = (await _orderService.GetAllAsync()).Take(10).ToList(),
                PendingOrders = (await _orderService.GetOrdersByStatusAsync("Pending")).Count()
            };

            return View(model);
        }

        #endregion

        #region Products Management

        // GET: /admin/products
        [HttpGet("products")]
        public async Task<IActionResult> Products(int page = 1, int pageSize = 10, string search = null, string metal = null, string stock = null)
        {
            // Get all products
            var allProducts = await _productService.GetAllAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                allProducts = allProducts.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(metal))
            {
                allProducts = allProducts.Where(p => p.Metal == metal).ToList();
            }

            if (!string.IsNullOrEmpty(stock))
            {
                allProducts = stock switch
                {
                    "instock" => allProducts.Where(p => p.Stock > 5).ToList(),
                    "lowstock" => allProducts.Where(p => p.Stock > 0 && p.Stock <= 5).ToList(),
                    "outofstock" => allProducts.Where(p => p.Stock == 0).ToList(),
                    _ => allProducts.ToList()
                };
            }

            var totalItems = allProducts.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            // Apply pagination
            var paginatedProducts = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View(paginatedProducts);
        }

        // POST: /admin/products/delete/{id}
        [HttpPost("products/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _productService.DeleteAsync(id);

            TempData["Success"] = success
                ? "Product deleted successfully."
                : "Error deleting product.";

            return RedirectToAction(nameof(Products));
        }

        #endregion

        #region Orders Management

        // GET: /admin/orders
        [HttpGet("orders")]
        public async Task<IActionResult> Orders(int page = 1, int pageSize = 10, string search = null, string status = null)
        {
            var allOrders = await _orderService.GetAllAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                if (int.TryParse(search, out int searchId))
                {
                    allOrders = allOrders.Where(o => o.Id == searchId || o.UserId.ToString().Contains(search)).ToList();
                }
            }

            if (!string.IsNullOrEmpty(status))
            {
                allOrders = allOrders.Where(o => o.Status == status).ToList();
            }

            var totalItems = allOrders.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var paginatedOrders = allOrders
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.CurrentStatus = status;

            return View(paginatedOrders);
        }

        #endregion

        #region Users Management

        // GET: /admin/users
        [HttpGet("users")]
        public async Task<IActionResult> Users(int page = 1, int pageSize = 10, string search = null, string role = null)
        {
            var allUsers = await _userService.GetAllAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                allUsers = allUsers.Where(u =>
                    u.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(role))
            {
                allUsers = allUsers.Where(u => u.Role == role).ToList();
            }

            var totalItems = allUsers.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var paginatedUsers = allUsers
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.CurrentRole = role;

            return View(paginatedUsers);
        }

        // GET: /admin/users/create
        [HttpGet("users/create")]
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: /admin/users/create
        [HttpPost("users/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _userService.GetByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already registered");
                return View(model);
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = hashedPassword,
                Role = model.Role ?? "Customer",
                CreatedAt = DateTime.UtcNow
            };

            var userId = await _userService.RegisterAsync(user);

            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Users));
        }

        // POST: /admin/users/change-role/{id}
        [HttpPost("users/change-role/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                TempData["Error"] = "Invalid role selected.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            user.Role = role;
            await _userService.UpdateAsync(user);

            TempData["Success"] = $"Role updated successfully for {user.Name}.";
            return RedirectToAction(nameof(Users));
        }

        // POST: /admin/users/delete/{id}
        [HttpPost("users/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteAsync(id);

            TempData["Success"] = success
                ? "User deleted successfully."
                : "Error deleting user.";

            return RedirectToAction(nameof(Users));
        }

        #endregion

        #region Categories Management

        // GET: /admin/categories
        [HttpGet("categories")]
        public async Task<IActionResult> Categories()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
        }

        // GET: /admin/categories/create
        [HttpGet("categories/create")]
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: /admin/categories/create
        [HttpPost("categories/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            await _categoryService.CreateAsync(category);
            TempData["Success"] = "Category created successfully.";
            return RedirectToAction(nameof(Categories));
        }

        // GET: /admin/categories/edit/{id}
        [HttpGet("categories/edit/{id}")]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction(nameof(Categories));
            }

            return View(category);
        }

        // POST: /admin/categories/edit/{id}
        [HttpPost("categories/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            category.Id = id;
            var success = await _categoryService.UpdateAsync(category);

            TempData["Success"] = success
                ? "Category updated successfully."
                : "Error updating category.";

            return RedirectToAction(nameof(Categories));
        }

        // POST: /admin/categories/delete/{id}
        [HttpPost("categories/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _categoryService.DeleteAsync(id);

            TempData["Success"] = success
                ? "Category deleted successfully."
                : "Error deleting category.";

            return RedirectToAction(nameof(Categories));
        }

        #endregion

        #region Reports Management

        // GET: /admin/reports
        [HttpGet("reports")]
        public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate, string type = "sales")
        {
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            var orders = await _orderService.GetOrdersByDateRangeAsync(startDate.Value, endDate.Value);

            if (orders == null || !orders.Any())
            {
                return View(new AdminSalesReportViewModel
                {
                    TotalRevenue = 0,
                    TotalOrders = 0,
                    Orders = new List<Order>(),
                    RevenueByStatus = new Dictionary<string, decimal>(),
                    OrdersByStatus = new Dictionary<string, int>()
                });
            }

            var totalRevenue = orders.Sum(o => o.Total);
            var totalOrders = orders.Count();

            var revenueByStatus = orders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.Total));

            var ordersByStatus = orders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var model = new AdminSalesReportViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                RevenueByStatus = revenueByStatus,
                OrdersByStatus = ordersByStatus,
                Orders = orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(20)
                    .ToList()
            };

            return View(model);
        }

        #endregion
    }
}
