using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
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

        #region Products Management

        // GET: /admin/products
        [HttpGet("products")]
        public async Task<IActionResult> Products()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        // GET: /admin/products/create
        [HttpGet("products/create")]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.Vendors = await _userService.GetVendorsAsync();
            return View();
        }

        // POST: /admin/products/create
        [HttpPost("products/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, int[] categoryIds)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                ViewBag.Vendors = await _userService.GetVendorsAsync();
                return View(product);
            }

            var productId = await _productService.CreateAsync(product);

            // Add categories
            if (categoryIds != null && categoryIds.Length > 0)
            {
                foreach (var categoryId in categoryIds)
                {
                    await _productService.AddCategoryToProductAsync(productId, categoryId);
                }
            }

            TempData["Success"] = "Producto creado exitosamente";
            return RedirectToAction(nameof(Products));
        }

        // GET: /admin/products/edit/{id}
        [HttpGet("products/edit/{id}")]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Producto no encontrado";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.ProductCategories = await _productService.GetProductCategoriesAsync(id);
            ViewBag.Vendors = await _userService.GetVendorsAsync();
            return View(product);
        }

        // POST: /admin/products/edit/{id}
        [HttpPost("products/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, int[] categoryIds)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                ViewBag.Vendors = await _userService.GetVendorsAsync();
                return View(product);
            }

            product.Id = id;
            var success = await _productService.UpdateAsync(product);

            if (!success)
            {
                TempData["Error"] = "Error al actualizar el producto";
                return View(product);
            }

            // Update categories
            var currentCategories = await _productService.GetProductCategoriesAsync(id);
            foreach (var category in currentCategories)
            {
                await _productService.RemoveCategoryFromProductAsync(id, category.Id);
            }

            if (categoryIds != null && categoryIds.Length > 0)
            {
                foreach (var categoryId in categoryIds)
                {
                    await _productService.AddCategoryToProductAsync(id, categoryId);
                }
            }

            TempData["Success"] = "Producto actualizado exitosamente";
            return RedirectToAction(nameof(Products));
        }

        // POST: /admin/products/delete/{id}
        [HttpPost("products/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _productService.DeleteAsync(id);

            if (success)
                TempData["Success"] = "Producto eliminado exitosamente";
            else
                TempData["Error"] = "Error al eliminar el producto";

            return RedirectToAction(nameof(Products));
        }

        #endregion

        #region Orders Management

        // GET: /admin/orders
        [HttpGet("orders")]
        public async Task<IActionResult> Orders(string status = null)
        {
            IEnumerable<Order> orders;

            if (!string.IsNullOrEmpty(status))
                orders = await _orderService.GetOrdersByStatusAsync(status);
            else
                orders = await _orderService.GetAllAsync();

            ViewBag.CurrentStatus = status;
            return View(orders);
        }

        // GET: /admin/orders/details/{id}
        [HttpGet("orders/details/{id}")]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _orderService.GetOrderWithDetailsAsync(id);

            if (order == null)
            {
                TempData["Error"] = "Orden no encontrada";
                return RedirectToAction(nameof(Orders));
            }

            return View(order);
        }

        // POST: /admin/orders/update-status/{id}
        [HttpPost("orders/update-status/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var success = await _orderService.UpdateStatusAsync(id, status);

            if (success)
                TempData["Success"] = "Estado de orden actualizado exitosamente";
            else
                TempData["Error"] = "Error al actualizar el estado";

            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        #endregion

        #region Users Management

        // GET: /admin/users
        [HttpGet("users")]
        public async Task<IActionResult> Users(string role = null)
        {
            IEnumerable<User> users;

            if (role == "Vendor")
                users = await _userService.GetVendorsAsync();
            else
                users = await _userService.GetAllAsync();

            if (!string.IsNullOrEmpty(role) && role != "Vendor")
                users = users.Where(u => u.Role == role);

            ViewBag.CurrentRole = role;
            return View(users);
        }

        // GET: /admin/users/details/{id}
        [HttpGet("users/details/{id}")]
        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado";
                return RedirectToAction(nameof(Users));
            }

            var orders = await _orderService.GetByUserIdAsync(id);
            ViewBag.Orders = orders;

            if (user.Role == "Vendor")
            {
                var products = await _productService.GetByVendorIdAsync(id);
                ViewBag.Products = products;
            }

            return View(user);
        }

        // POST: /admin/users/delete/{id}
        [HttpPost("users/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteAsync(id);

            if (success)
                TempData["Success"] = "Usuario eliminado exitosamente";
            else
                TempData["Error"] = "Error al eliminar el usuario";

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
            TempData["Success"] = "Categoría creada exitosamente";
            return RedirectToAction(nameof(Categories));
        }

        // GET: /admin/categories/edit/{id}
        [HttpGet("categories/edit/{id}")]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category == null)
            {
                TempData["Error"] = "Categoría no encontrada";
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

            if (success)
                TempData["Success"] = "Categoría actualizada exitosamente";
            else
                TempData["Error"] = "Error al actualizar la categoría";

            return RedirectToAction(nameof(Categories));
        }

        // POST: /admin/categories/delete/{id}
        [HttpPost("categories/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _categoryService.DeleteAsync(id);

            if (success)
                TempData["Success"] = "Categoría eliminada exitosamente";
            else
                TempData["Error"] = "Error al eliminar la categoría";

            return RedirectToAction(nameof(Categories));
        }

        #endregion

        #region Reviews Management

        // GET: /admin/reviews
        [HttpGet("reviews")]
        public async Task<IActionResult> Reviews()
        {
            // Obtener todas las reviews - necesitarías un método GetAllAsync en IReviewService
            return View();
        }

        // POST: /admin/reviews/delete/{id}
        [HttpPost("reviews/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var success = await _reviewService.DeleteAsync(id);

            if (success)
                TempData["Success"] = "Reseña eliminada exitosamente";
            else
                TempData["Error"] = "Error al eliminar la reseña";

            return RedirectToAction(nameof(Reviews));
        }

        #endregion

        #region Reports

        // GET: /admin/reports/sales
        [HttpGet("reports/sales")]
        public async Task<IActionResult> SalesReport()
        {
            var totalRevenue = await _orderService.GetTotalRevenueAsync();
            var totalOrders = await _orderService.GetTotalOrdersCountAsync();
            var orders = await _orderService.GetAllAsync();

            var model = new SalesReportViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                Orders = orders.Where(o => o.Status != "Cart").ToList()
            };

            return View(model);
        }

        #endregion
    }

    // ViewModels
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalVendors { get; set; }
        public int PendingOrders { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
    }

    public class SalesReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}