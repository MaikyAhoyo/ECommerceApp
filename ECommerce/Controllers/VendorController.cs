using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;
using ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        #region Dashboard

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
                RecentOrders = vendorOrders.OrderByDescending(o => o.OrderDate).Take(5).ToList(),
                TopProducts = productsList.OrderByDescending(p => p.Price).Take(5).ToList()
            };

            return View(model);
        }

        #endregion

        #region My Products

        // GET: /admin/products
        [HttpGet("products")]
        public async Task<IActionResult> MyProducts(int page = 1, int pageSize = 10, string search = null, string metal = null, string stock = null)
        {
            // Get all products
            int vendorId = GetCurrentVendorId();
            var products = await _productService.GetByVendorIdAsync(vendorId);

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(metal))
            {
                products = products.Where(p => p.Metal == metal).ToList();
            }

            if (!string.IsNullOrEmpty(stock))
            {
                products = stock switch
                {
                    "instock" => products.Where(p => p.Stock > 5).ToList(),
                    "lowstock" => products.Where(p => p.Stock > 0 && p.Stock <= 5).ToList(),
                    "outofstock" => products.Where(p => p.Stock == 0).ToList(),
                    _ => products.ToList()
                };
            }

            var totalItems = products.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            // Apply pagination
            var paginatedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View(paginatedProducts);
        }

        // GET: /vendor/products/create
        [HttpGet("products/create")]
        public async Task<IActionResult> CreateProduct()
        {
            var categories = await _categoryService.GetAllAsync();

            var model = new ProductCreateViewModel
            {
                AvailableCategories = categories?.ToList() ?? new List<Category>()
            };

            return View(model);
        }

        // POST: /vendor/products/create
        [HttpPost("products/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCategories = (await _categoryService.GetAllAsync()).ToList();
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                OriginalPrice = model.Price,
                Stock = model.Stock,
                VendorId = GetCurrentVendorId(),
                ImageUrl = model.ImageUrl,
                Metal = model.Metal,
                Purity = model.Purity
            };

            var productId = await _productService.CreateAsync(product);

            if (model.SelectedCategoryIds?.Any() == true)
            {
                foreach (var categoryId in model.SelectedCategoryIds)
                    await _productService.AddCategoryToProductAsync(productId, categoryId);
            }

            TempData["Success"] = "Product created successfully";
            return RedirectToAction(nameof(MyProducts));
        }

        // GET: /vendor/products/edit/{id}
        [HttpGet("products/edit/{id}")]
        public async Task<IActionResult> EditProduct(int id)
        {
            var vendorId = GetCurrentVendorId();
            var product = await _productService.GetByIdAsync(id);

            if (product == null || product.VendorId != vendorId)
            {
                TempData["Error"] = "Product not found or no permission for editing granted.";
                return RedirectToAction(nameof(MyProducts));
            }

            var categories = await _categoryService.GetAllAsync();
            var productCategories = await _productService.GetProductCategoriesAsync(id);

            var viewModel = new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                Metal = product.Metal,
                Purity = product.Purity,
                AvailableCategories = categories.ToList(),
                SelectedCategoryIds = productCategories.Select(c => c.Id).ToList()
            };

            return View(viewModel);
        }

        // POST: /vendor/products/edit/{id}
        [HttpPost("products/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, ProductEditViewModel model)
        {
            var vendorId = GetCurrentVendorId();
            var existingProduct = await _productService.GetByIdAsync(id);

            if (existingProduct == null || existingProduct.VendorId != vendorId)
            {
                TempData["Error"] = "Product not found or no permission for editing granted.";
                return RedirectToAction(nameof(MyProducts));
            }

            if (!ModelState.IsValid)
            {
                model.AvailableCategories = (await _categoryService.GetAllAsync()).ToList();
                return View(model);
            }

            existingProduct.Name = model.Name;
            existingProduct.Description = model.Description;
            existingProduct.Price = model.Price;
            existingProduct.Stock = model.Stock;
            existingProduct.ImageUrl = model.ImageUrl;
            existingProduct.Metal = model.Metal;
            existingProduct.Purity = model.Purity;

            var success = await _productService.UpdateAsync(existingProduct);

            if (!success)
            {
                TempData["Error"] = "Error updating product.";
                model.AvailableCategories = (await _categoryService.GetAllAsync()).ToList();
                return View(model);
            }

            var currentCategories = await _productService.GetProductCategoriesAsync(id);
            foreach (var category in currentCategories)
                await _productService.RemoveCategoryFromProductAsync(id, category.Id);

            if (model.SelectedCategoryIds?.Any() == true)
            {
                foreach (var categoryId in model.SelectedCategoryIds)
                    await _productService.AddCategoryToProductAsync(id, categoryId);
            }

            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(MyProducts));
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
                TempData["Error"] = "Product not found or no permission for deleting granted";
                return RedirectToAction(nameof(MyProducts));
            }

            var success = await _productService.DeleteAsync(id);

            if (success)
                TempData["Success"] = "Producto deleted successfully";
            else
                TempData["Error"] = "Error deleting product";

            return RedirectToAction(nameof(MyProducts));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string newStatus)
        {
            int vendorId = GetCurrentVendorId();

            // 1) Buscar la orden
            var order = await _orderService.GetOrderWithDetailsAsync(orderId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(MyOrders));
            }

            // 2) Validar que la orden pertenezca al vendor
            var vendorProducts = await _productService.GetByVendorIdAsync(vendorId);
            var vendorProductIds = vendorProducts.Select(p => p.Id).ToList();

            bool belongsToVendor = order.OrderItems.Any(oi => vendorProductIds.Contains(oi.ProductId));

            if (!belongsToVendor)
            {
                TempData["Error"] = "You don't have permission to update this order.";
                return RedirectToAction(nameof(MyOrders));
            }

            // 3) Validar estatus permitido
            var allowedStatuses = new List<string> { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

            if (!allowedStatuses.Contains(newStatus))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction(nameof(MyOrders));
            }

            // 4) Actualizar
            order.Status = newStatus;

            var updated = await _orderService.UpdateAsync(order);

            if (!updated)
            {
                TempData["Error"] = "Could not update order status.";
                return RedirectToAction(nameof(MyOrders));
            }

            TempData["Success"] = $"Order status updated to {newStatus}.";
            return RedirectToAction(nameof(MyOrders));
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
                TempData["Error"] = "Product not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(MyProducts));
            }

            try
            {
                var success = await _productService.UpdateStockAsync(id, quantity);

                if (!success)
                {
                    TempData["Error"] = "An error occurred while updating the stock.";
                    return RedirectToAction(nameof(MyProducts));
                }

                TempData["Success"] = "Stock updated successfully!";
                return RedirectToAction(nameof(MyProducts));
            }
            catch (Exception)
            {
                TempData["Error"] = "An unexpected error occurred while updating the stock.";
                return RedirectToAction(nameof(MyProducts));
            }
        }

        // POST: /vendor/products/discount/{id}
        [HttpPost("products/discount/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyDiscount(int id, decimal discount)
        {
            int vendorId = GetCurrentVendorId();
            var product = await _productService.GetByIdAsync(id);

            if (product == null || product.VendorId != vendorId)
            {
                TempData["Error"] = "Product not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(MyProducts));
            }

            if (discount < 0 || discount > 100)
            {
                TempData["Error"] = "Discount must be between 0% and 100%.";
                return RedirectToAction(nameof(MyProducts));
            }

            try
            {
                var success = await _productService.UpdateDiscountAsync(id, discount);

                if (!success)
                {
                    TempData["Error"] = "An error occurred while updating the discount.";
                    return RedirectToAction(nameof(MyProducts));
                }

                TempData["Success"] = "Discount updated successfully!";
                return RedirectToAction(nameof(MyProducts));
            }
            catch (Exception)
            {
                TempData["Error"] = "An unexpected error occurred while applying the discount.";
                return RedirectToAction(nameof(MyProducts));
            }
        }

        #endregion

        #region Orders

        // GET: /vendor/orders
        [HttpGet("orders")]
        public async Task<IActionResult> MyOrders(int page = 1, int pageSize = 10, string search = null, string status = null)
        {
            int vendorId = GetCurrentVendorId();
            var products = await _productService.GetByVendorIdAsync(vendorId);
            var productIds = products.Select(p => p.Id).ToList();

            var allOrders = await _orderService.GetAllAsync();
            var myOrders = new List<Order>();

            foreach (var order in allOrders)
            {
                if (order.Status == "Cart") continue;

                var orderWithDetails = await _orderService.GetOrderWithDetailsAsync(order.Id);
                if (orderWithDetails != null && orderWithDetails.OrderItems.Any(oi => productIds.Contains(oi.ProductId)))
                {
                    myOrders.Add(orderWithDetails);
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                if (int.TryParse(search, out int searchId))
                {
                    myOrders = myOrders.Where(o => o.Id == searchId || o.UserId.ToString().Contains(search)).ToList();
                }
            }

            if (!string.IsNullOrEmpty(status))
            {
                myOrders = myOrders.Where(o => o.Status == status).ToList();
            }

            var totalItems = myOrders.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var paginatedOrders = myOrders
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

        #region Reviews

        // GET: /vendor/reviews
        [HttpGet("reviews")]
        public async Task<IActionResult> Reviews(string search = "", int? productId = null, int page = 1, int pageSize = 10)
        {
            int vendorId = GetCurrentVendorId();
            var products = await _productService.GetByVendorIdAsync(vendorId);
            var allReviews = new List<Review>();

            foreach (var product in products)
            {
                var reviews = await _reviewService.GetByProductIdAsync(product.Id);

                foreach (var review in reviews)
                {
                    if (review.User == null)
                        review.User = await _userService.GetByIdAsync(review.UserId);
                    if (review.Product == null)
                        review.Product = product;
                }

                allReviews.AddRange(reviews);
            }

            if (productId.HasValue)
                allReviews = allReviews.Where(r => r.Product?.Id == productId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                allReviews = allReviews.Where(r =>
                    (r.User?.Name?.ToLower().Contains(lower) ?? false) ||
                    (r.User?.Email?.ToLower().Contains(lower) ?? false) ||
                    (r.Product?.Name?.ToLower().Contains(lower) ?? false) ||
                    (r.Comment?.ToLower().Contains(lower) ?? false)
                ).ToList();
            }

            allReviews = allReviews.OrderByDescending(r => r.CreatedAt).ToList();

            int totalItems = allReviews.Count;
            double averageRating = totalItems > 0 ? allReviews.Average(r => r.Rating) : 0;

            var pagedReviews = allReviews
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.AverageRating = averageRating;
            ViewBag.Search = search;
            ViewBag.Products = products;
            ViewBag.SelectedProductId = productId;

            return View(pagedReviews);

        }

        #endregion

        #region Reports

        [HttpGet("reports")]
        public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate, string type = "sales")
        {
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            var orders = await _orderService.GetOrdersWithItemsByDateRangeAsync(startDate.Value, endDate.Value);

            if (orders == null || !orders.Any())
            {
                return View(new VendorSalesReportViewModel
                {
                    TotalRevenue = 0,
                    TotalOrders = 0,
                    TotalProductsSold = 0,
                    Orders = new List<Order>(),
                    ProductsSoldByMonth = new Dictionary<string, int>(),
                    TopSellingProducts = new List<ProductSalesData>()
                });
            }

            var totalRevenue = orders.Sum(o => o.Total);
            var totalOrders = orders.Count();

            var totalProductsSold = orders
                .SelectMany(o => o.OrderItems)
                .Sum(oi => oi.Quantity);

            var productsSoldByMonth = orders
                .GroupBy(o => o.OrderDate.ToString("MMM yyyy"))
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity)
                );

            var topSellingProducts = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new ProductSalesData
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(5)
                .ToList();

            var model = new VendorSalesReportViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProductsSold = totalProductsSold,
                Orders = orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(20)
                    .ToList(),
                ProductsSoldByMonth = productsSoldByMonth,
                TopSellingProducts = topSellingProducts
            };

            return View(model);
        }

        #endregion

        #region Helper Methods

        private int GetCurrentVendorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new Exception("Couldn't get current user ID. Be sure you're loged in.");

            if (int.TryParse(userIdClaim.Value, out int userId))
                return userId;

            throw new Exception("User ID it's not a valid format.");
        }

        #endregion
    }
}