using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    [Route("vendor")]
    public class VendorController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICategoryService _categoryService;
        private readonly IReviewService _reviewService;
        private readonly IUserService _userService;

        public VendorController(
            IProductService productService,
            IOrderService orderService,
            ICategoryService categoryService,
            IReviewService reviewService,
            IUserService userService)
        {
            _productService = productService;
            _orderService = orderService;
            _categoryService = categoryService;
            _reviewService = reviewService;
            _userService = userService;
        }

        // GET: /vendor/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // Obtener el vendorId de la sesión/claims - por ahora demo
            int vendorId = GetCurrentVendorId();

            var products = await _productService.GetByVendorIdAsync(vendorId);
            var productsList = products.ToList();

            // Calcular estadísticas
            var totalProducts = productsList.Count;
            var totalStock = productsList.Sum(p => p.Stock);
            var totalValue = productsList.Sum(p => p.Price * p.Stock);

            // Obtener órdenes que contienen productos del vendedor
            var allOrders = await _orderService.GetAllAsync();
            var vendorOrders = new List<Order>();

            foreach (var order in allOrders)
            {
                var orderWithDetails = await _orderService.GetOrderWithDetailsAsync(order.Id);
                if (orderWithDetails != null && orderWithDetails.OrderItems.Any(oi => productsList.Any(p => p.Id == oi.ProductId)))
                {
                    vendorOrders.Add(orderWithDetails);
                }
            }

            var model = new VendorDashboardViewModel
            {
                VendorId = vendorId,
                TotalProducts = totalProducts,
                TotalStock = totalStock,
                TotalInventoryValue = totalValue,
                LowStockProducts = productsList.Where(p => p.Stock < 5).Count(),
                OutOfStockProducts = productsList.Where(p => p.Stock == 0).Count(),
                RecentOrders = vendorOrders.OrderByDescending(o => o.OrderDate).Take(10).ToList(),
                TopProducts = productsList.OrderByDescending(p => p.Price).Take(5).ToList()
            };

            return View(model);
        }

        #region My Products

        // GET: /vendor/products
        [HttpGet("products")]
        public async Task<IActionResult> MyProducts()
        {
            int vendorId = GetCurrentVendorId();
            var products = await _productService.GetByVendorIdAsync(vendorId);
            return View(products);
        }

        // GET: /vendor/products/create
        [HttpGet("products/create")]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View();
        }

        // POST: /vendor/products/create
        [HttpPost("products/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, int[] categoryIds)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                return View(product);
            }

            product.VendorId = GetCurrentVendorId();
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
            return RedirectToAction(nameof(MyProducts));
        }

        // GET: /vendor/products/edit/{id}
        [HttpGet("products/edit/{id}")]
        public async Task<IActionResult> EditProduct(int id)
        {
            int vendorId = GetCurrentVendorId();
            var product = await _productService.GetByIdAsync(id);

            if (product == null || product.VendorId != vendorId)
            {
                TempData["Error"] = "Producto no encontrado o no tienes permiso para editarlo";
                return RedirectToAction(nameof(MyProducts));
            }

            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.ProductCategories = await _productService.GetProductCategoriesAsync(id);
            return View(product);
        }

        // POST: /vendor/products/edit/{id}
        [HttpPost("products/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, int[] categoryIds)
        {
            int vendorId = GetCurrentVendorId();
            var existingProduct = await _productService.GetByIdAsync(id);

            if (existingProduct == null || existingProduct.VendorId != vendorId)
            {
                TempData["Error"] = "Producto no encontrado o no tienes permiso para editarlo";
                return RedirectToAction(nameof(MyProducts));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                return View(product);
            }

            product.Id = id;
            product.VendorId = vendorId;
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
            return RedirectToAction(nameof(MyProducts));
        }

        // GET: /vendor/products/details/{id}
        [HttpGet("products/details/{id}")]
        public async Task<IActionResult> ProductDetails(int id)
        {
            int vendorId = GetCurrentVendorId();
            var product = await _productService.GetByIdAsync(id);

            if (product == null || product.VendorId != vendorId)
            {
                TempData["Error"] = "Producto no encontrado";
                return RedirectToAction(nameof(MyProducts));
            }

            var reviews = await _reviewService.GetByProductIdAsync(id);
            var avgRating = await _reviewService.GetAverageRatingByProductAsync(id);
            var categories = await _productService.GetProductCategoriesAsync(id);

            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = avgRating;
            ViewBag.Categories = categories;

            return View(product);
        }

        // POST: /vendor/products/delete/{id}
        [HttpPost("products/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            int vendorId = GetCurrentVendorId();
            var product = await _productService.GetByIdAsync(id);

            if (product == null || product.VendorId != vendorId)
            {
                TempData["Error"] = "Producto no encontrado o no tienes permiso para eliminarlo";
                return RedirectToAction(nameof(MyProducts));
            }

            var success = await _productService.DeleteAsync(id);

            if (success)
                TempData["Success"] = "Producto eliminado exitosamente";
            else
                TempData["Error"] = "Error al eliminar el producto";

            return RedirectToAction(nameof(MyProducts));
        }

        // POST: /vendor/products/update-stock/{id}
        [HttpPost("products/update-stock/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int id, int quantity)
        {
            int vendorId = GetCurrentVendorId();
            var product = await _productService.GetByIdAsync(id);

            if (product == null || product.VendorId != vendorId)
            {
                return Json(new { success = false, message = "Producto no encontrado" });
            }

            var success = await _productService.UpdateStockAsync(id, quantity);

            if (success)
                return Json(new { success = true, message = "Stock actualizado" });
            else
                return Json(new { success = false, message = "Error al actualizar stock" });
        }

        #endregion

        #region Inventory Management

        // GET: /vendor/inventory
        [HttpGet("inventory")]
        public async Task<IActionResult> Inventory()
        {
            int vendorId = GetCurrentVendorId();
            var products = await _productService.GetByVendorIdAsync(vendorId);

            var model = new VendorInventoryViewModel
            {
                AllProducts = products.ToList(),
                LowStockProducts = products.Where(p => p.Stock > 0 && p.Stock < 5).ToList(),
                OutOfStockProducts = products.Where(p => p.Stock == 0).ToList()
            };

            return View(model);
        }

        #endregion

        #region Orders

        // GET: /vendor/orders
        [HttpGet("orders")]
        public async Task<IActionResult> Orders()
        {
            int vendorId = GetCurrentVendorId();
            var products = await _productService.GetByVendorIdAsync(vendorId);
            var productIds = products.Select(p => p.Id).ToList();

            // Obtener todas las órdenes y filtrar las que contienen productos del vendedor
            var allOrders = await _orderService.GetAllAsync();
            var vendorOrders = new List<Order>();

            foreach (var order in allOrders)
            {
                if (order.Status == "Cart") continue;

                var orderWithDetails = await _orderService.GetOrderWithDetailsAsync(order.Id);
                if (orderWithDetails != null && orderWithDetails.OrderItems.Any(oi => productIds.Contains(oi.ProductId)))
                {
                    vendorOrders.Add(orderWithDetails);
                }
            }

            return View(vendorOrders.OrderByDescending(o => o.OrderDate));
        }

        // GET: /vendor/orders/details/{id}
        [HttpGet("orders/details/{id}")]
        public async Task<IActionResult> OrderDetails(int id)
        {
            int vendorId = GetCurrentVendorId();
            var products = await _productService.GetByVendorIdAsync(vendorId);
            var productIds = products.Select(p => p.Id).ToList();

            var order = await _orderService.GetOrderWithDetailsAsync(id);

            if (order == null || !order.OrderItems.Any(oi => productIds.Contains(oi.ProductId)))
            {
                TempData["Error"] = "Orden no encontrada";
                return RedirectToAction(nameof(Orders));
            }

            // Filtrar solo los items del vendedor
            order.OrderItems = order.OrderItems.Where(oi => productIds.Contains(oi.ProductId)).ToList();

            return View(order);
        }

        #endregion

        #region Reviews

        // GET: /vendor/reviews
        [HttpGet("reviews")]
        public async Task<IActionResult> Reviews()
        {
            int vendorId = GetCurrentVendorId();
            var products = await _productService.GetByVendorIdAsync(vendorId);
            var allReviews = new List<Review>();

            foreach (var product in products)
            {
                var reviews = await _reviewService.GetByProductIdAsync(product.Id);
                allReviews.AddRange(reviews);
            }

            return View(allReviews.OrderByDescending(r => r.CreatedAt));
        }

        #endregion

        #region Reports

        // GET: /vendor/reports/sales
        [HttpGet("reports/sales")]
        public async Task<IActionResult> SalesReport()
        {
            int vendorId = GetCurrentVendorId();
            var products = await _productService.GetByVendorIdAsync(vendorId);
            var productIds = products.Select(p => p.Id).ToList();

            var allOrders = await _orderService.GetAllAsync();
            var vendorOrders = new List<Order>();
            decimal totalRevenue = 0;

            foreach (var order in allOrders)
            {
                if (order.Status == "Cart" || order.Status == "Cancelled") continue;

                var orderWithDetails = await _orderService.GetOrderWithDetailsAsync(order.Id);
                if (orderWithDetails != null && orderWithDetails.OrderItems.Any(oi => productIds.Contains(oi.ProductId)))
                {
                    // Calcular solo el revenue de productos del vendedor
                    var vendorItems = orderWithDetails.OrderItems.Where(oi => productIds.Contains(oi.ProductId));
                    totalRevenue += vendorItems.Sum(oi => oi.Quantity * oi.UnitPrice);
                    vendorOrders.Add(orderWithDetails);
                }
            }

            var model = new VendorSalesReportViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = vendorOrders.Count,
                TotalProductsSold = vendorOrders.SelectMany(o => o.OrderItems).Where(oi => productIds.Contains(oi.ProductId)).Sum(oi => oi.Quantity),
                Orders = vendorOrders.OrderByDescending(o => o.OrderDate).ToList()
            };

            return View(model);
        }

        #endregion

        #region Helper Methods

        private int GetCurrentVendorId()
        {
            // TODO: Obtener el ID del vendedor desde Claims/Session
            // Por ahora retornamos un ID de ejemplo
            return 2;
        }

        #endregion
    }

    // ViewModels
    public class VendorDashboardViewModel
    {
        public int VendorId { get; set; }
        public int TotalProducts { get; set; }
        public int TotalStock { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Product> TopProducts { get; set; } = new List<Product>();
    }

    public class VendorInventoryViewModel
    {
        public List<Product> AllProducts { get; set; } = new List<Product>();
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
        public List<Product> OutOfStockProducts { get; set; } = new List<Product>();
    }

    public class VendorSalesReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProductsSold { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}