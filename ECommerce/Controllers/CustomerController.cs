using ECommerce.Models.Entities;
using ECommerce.Services.Implementations;
using ECommerce.Services.Interfaces;
using ECommerce.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;


namespace ECommerce.Controllers
{
    [Route("customer")]
    public class CustomerController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IReviewService _reviewService;
        private readonly IShippingAddressService _shippingAddressService;
        private readonly IPaymentService _paymentService;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            IProductService productService,
            IOrderService orderService,
            IReviewService reviewService,
            IShippingAddressService shippingAddressService,
            IPaymentService paymentService,
            ICategoryService categoryService,
            IUserService userService,
            ILogger<CustomerController> logger)
        {
            _productService = productService;
            _orderService = orderService;
            _reviewService = reviewService;
            _shippingAddressService = shippingAddressService;
            _paymentService = paymentService;
            _categoryService = categoryService;
            _userService = userService;
            _logger = logger;
        }

        #region Home & Products

        // GET: /customer/home
        [HttpGet("home")]
        public async Task<IActionResult> Home()
        {
            var featuredProducts = (await _productService.GetTop4Async()).OrderByDescending(p => p.Id).ToList();
            var categories = (await _categoryService.GetAllAsync()).ToList();

            foreach (var p in featuredProducts)
            {
                var productCategories = await _categoryService.GetByProductIdAsync(p.Id);
                p.CategoryNames = string.Join(", ", productCategories.Select(c => ((Category)c).Name));
            }

            var model = new CustomerHomeViewModel
            {
                FeaturedProducts = featuredProducts,
                Categories = categories,
            };

            return View(model);
        }

        // GET: /customer/products
        [HttpGet("products")]
        public async Task<IActionResult> Products(
        string? metal = null,
        int? categoryId = null,
        string? search = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sortBy = null,
        int page = 1,
        int pageSize = 12)
        {
            var allProducts = await _productService.GetAllAsync();

            var products = new List<Product>();

            foreach (var p in allProducts)
            {
                var productCategories = await _categoryService.GetByProductIdAsync(p.Id);
                p.CategoryNames = string.Join(", ", productCategories.Select(c => ((Category)c).Name));

                if (categoryId.HasValue) { 
                    if (productCategories.Any(c => c.Id == categoryId.Value))
                    {
                        products.Add(p);
                    }
                }
                else
                {
                    products.Add(p);
                }
            }

            if (!string.IsNullOrEmpty(search))
                products = products
                    .Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(metal))
                products = products
                    .Where(p => p.Metal != null && p.Metal.Equals(metal, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (minPrice.HasValue)
                products = products.Where(p => p.Price >= minPrice.Value).ToList();

            if (maxPrice.HasValue)
                products = products.Where(p => p.Price <= maxPrice.Value).ToList();

            products = sortBy switch
            {
                "price_asc" => products.OrderBy(p => p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                "name_asc" => products.OrderBy(p => p.Name).ToList(),
                "name_desc" => products.OrderByDescending(p => p.Name).ToList(),
                _ => products
            };

            int totalProducts = products.Count;
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var paginatedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var categories = (await _categoryService.GetAllAsync()).ToList();

            ViewBag.PageSize = pageSize;

            var viewModel = new CustomerProductsViewModel
            {
                Products = paginatedProducts,
                Categories = categories,
                CurrentMetal = metal,
                CurrentCategoryId = categoryId,
                CurrentMinPrice = minPrice,
                CurrentMaxPrice = maxPrice,
                CurrentSortBy = sortBy,
                CurrentSearch = search,
                SearchTerm = search,
                TotalProducts = totalProducts,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: /customer/products/details/{id}
        [HttpGet("products/details/{id}")]
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction(nameof(Products));
            }

            var productCategories = await _categoryService.GetByProductIdAsync(product.Id);
            product.CategoryNames = string.Join(", ", productCategories.Select(c => ((Category)c).Name));

            var reviews = await _reviewService.GetByProductIdAsync(id);
            var avgRating = await _reviewService.GetAverageRatingByProductAsync(id);
            var reviewCount = await _reviewService.GetReviewCountByProductAsync(id);
            var categories = await _productService.GetProductCategoriesAsync(id);

            var relatedProducts = (await _productService.GetAllAsync())
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToList();

            var userLoggedIn = User.Identity != null && User.Identity.IsAuthenticated;

            var viewModel = new CustomerProductDetailsViewModel
            {
                LoggedIn = userLoggedIn,
                Product = product,
                Reviews = reviews.Cast<Review>().ToList(),
                AverageRating = avgRating,
                ReviewCount = reviewCount,
                Categories = categories.Cast<Category>().ToList(),
                RelatedProducts = relatedProducts,
                InStock = product.Stock > 0
            };

            return View(viewModel);
        }

        #endregion

        #region Shopping Cart

        // GET: /customer/cart
        [HttpGet("cart")]
        public async Task<IActionResult> Cart()
        {
            int userId = GetCurrentUserId();
            var cart = await _orderService.GetActiveCartAsync(userId);

            if (cart == null)
            {
                var newCart = new Order
                {
                    UserId = userId,
                    Status = "Cart",
                    OrderDate = DateTime.UtcNow,
                    Total = 0
                };
                var cartId = await _orderService.CreateAsync(newCart);
                cart = await _orderService.GetOrderWithDetailsAsync(cartId);
            }
            else
            {
                cart = await _orderService.GetOrderWithDetailsAsync(cart.Id);
            }

            var viewModel = new CustomerCartViewModel();
            if (cart != null)
            {
                viewModel.Cart = cart;

                if (cart.OrderItems != null && cart.OrderItems.Any())
                {
                    foreach (var oi in cart.OrderItems)
                    {
                        var itemVm = new CartItemViewModel
                        {
                            Id = oi.Id,
                            ProductId = oi.ProductId,
                            ProductName = oi.Product?.Name ?? string.Empty,
                            ProductImageUrl = oi.Product?.ImageUrl ?? string.Empty,
                            Metal = oi.Product?.Metal ?? string.Empty,
                            Purity = oi.Product?.Purity ?? 0,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            MaxStock = oi.Product != null ? oi.Product.Stock : 0
                        };
                        viewModel.Items.Add(itemVm);
                    }
                }

                viewModel.Subtotal = await _orderService.CalculateOrderTotalAsync(cart.Id);
                viewModel.Tax = Math.Round(viewModel.Subtotal * 0.16m, 2);
                viewModel.Shipping = 0m;
                viewModel.Total = viewModel.Subtotal + viewModel.Tax + viewModel.Shipping;
                viewModel.ItemCount = viewModel.Items.Sum(i => i.Quantity);
            }

            return View(viewModel);
        }

        // POST: /customer/cart/add
        [HttpPost("cart/add")]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            int userId = GetCurrentUserId();

            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Products));
            }

            var cart = await _orderService.GetActiveCartAsync(userId);
            int currentQuantityInCart = 0;

            if (cart != null)
            {
                var cartItems = await _orderService.GetOrderItemsAsync(cart.Id);
                var existingItem = cartItems.FirstOrDefault(i => i.ProductId == productId);
                if (existingItem != null)
                {
                    currentQuantityInCart = existingItem.Quantity;
                }
            }

            if (currentQuantityInCart + quantity > product.Stock)
            {
                TempData["Error"] = $"Insufficient stock. Only {product.Stock} left.";
                return RedirectToAction(nameof(ProductDetails), new { id = productId });
            }

            if (cart == null)
            {
                var newCart = new Order
                {
                    UserId = userId,
                    Status = "Cart",
                    OrderDate = DateTime.UtcNow,
                    Total = 0
                };
                var cartId = await _orderService.CreateAsync(newCart);
                cart = await _orderService.GetByIdAsync(cartId);
            }

            var item = new OrderItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price
            };

            var success = await _orderService.AddItemToOrderAsync(cart.Id, item);

            if (success)
            {
                var total = await _orderService.CalculateOrderTotalAsync(cart.Id);
                cart.Total = total;
                await _orderService.UpdateAsync(cart);

                TempData["Success"] = "Product successfully added to the cart";
                return RedirectToAction(nameof(Cart));
            }

            TempData["Error"] = "There was a problem adding the product.";
            return RedirectToAction(nameof(ProductDetails), new { id = productId });
        }

        // POST: /customer/cart/update
        [HttpPost("cart/update")]
        public async Task<IActionResult> UpdateCartItem(int itemId, int quantity)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Quantity must be greater than 0.";
                return RedirectToAction(nameof(Cart));
            }

            try
            {
                int userId = GetCurrentUserId();
                var cart = await _orderService.GetActiveCartAsync(userId);

                if (cart == null)
                {
                    TempData["Error"] = "Cart not found.";
                    return RedirectToAction(nameof(Home));
                }

                var cartItems = await _orderService.GetOrderItemsAsync(cart.Id);
                var itemToUpdate = cartItems.FirstOrDefault(i => i.Id == itemId);

                if (itemToUpdate == null)
                {
                    TempData["Error"] = "Product not in cart.";
                    return RedirectToAction(nameof(Cart));
                }

                if (itemToUpdate.Product != null && quantity > itemToUpdate.Product.Stock)
                {
                    TempData["Error"] = $"Insufficient stock. Maximum available: {itemToUpdate.Product.Stock}.";
                    return RedirectToAction(nameof(Cart));
                }

                var success = await _orderService.UpdateOrderItemQuantityAsync(itemId, quantity);

                if (success)
                {
                    TempData["Success"] = "Cart updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Could not update quantity.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }

            return RedirectToAction(nameof(Cart));
        }

        // POST: /customer/cart/remove
        [HttpPost("cart/remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            var success = await _orderService.RemoveItemFromOrderAsync(itemId);

            if (success)
            {
                TempData["Success"] = "Product successfully removed from the cart.";
            }
            else
            {
                TempData["Error"] = "Error removing product.";
            }

            return RedirectToAction(nameof(Cart));
        }

        #endregion

        #region Checkout & Orders

        // GET: /customer/checkout/address-selection
        [HttpGet("checkout/address-selection")]
        public async Task<IActionResult> CheckoutAddressSelection()
        {
            int userId = GetCurrentUserId();
            var cart = await _orderService.GetActiveCartAsync(userId);

            if (cart == null)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction(nameof(Cart));
            }

            cart = await _orderService.GetOrderWithDetailsAsync(cart.Id);

            if (cart.OrderItems == null || !cart.OrderItems.Any())
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction(nameof(Cart));
            }

            var addresses = await _shippingAddressService.GetByUserIdAsync(userId);

            // Build view model
            var subtotal = await _orderService.CalculateOrderTotalAsync(cart.Id);
            var tax = Math.Round(subtotal * 0.16m, 2);
            var shipping = 0m;

            var viewModel = new CheckoutAddressSelectionViewModel
            {
                Cart = cart,
                Addresses = addresses.ToList(),
                Subtotal = subtotal,
                Tax = tax,
                Shipping = shipping,
                Total = subtotal + tax + shipping
            };

            return View(viewModel);
        }

        // GET: /customer/checkout
        [HttpGet("checkout")]
        public async Task<IActionResult> Checkout()
        {
            int userId = GetCurrentUserId();
            var cart = await _orderService.GetActiveCartAsync(userId);

            if (cart == null)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction(nameof(Cart));
            }

            cart = await _orderService.GetOrderWithDetailsAsync(cart.Id);

            if (cart.OrderItems == null || !cart.OrderItems.Any())
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction(nameof(Cart));
            }

            var addresses = await _shippingAddressService.GetByUserIdAsync(userId);

            var subtotal = await _orderService.CalculateOrderTotalAsync(cart.Id);
            var tax = Math.Round(subtotal * 0.16m, 2);
            var shipping = 0m;

            var viewModel = new CustomerCheckoutViewModel
            {
                Cart = cart,
                Addresses = addresses.ToList(),
                Subtotal = subtotal,
                Tax = tax,
                Shipping = shipping,
                Total = subtotal + tax + shipping
            };

            return View(viewModel);
        }

        // POST: /customer/checkout/process
        [HttpPost("checkout/process")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(int shippingAddressId, string paymentMethod)
        {
            int userId = GetCurrentUserId();
            var cart = await _orderService.GetActiveCartAsync(userId);

            if (cart == null)
            {
                TempData["Error"] = "Cart not found";
                return RedirectToAction(nameof(Cart));
            }

            var success = await _orderService.CheckoutOrderAsync(cart.Id);

            if (!success)
            {
                TempData["Error"] = "Error processing order";
                return RedirectToAction(nameof(CheckoutAddressSelection));
            }

            var order = await _orderService.GetByIdAsync(cart.Id);
            if (order != null)
            {
                order.AddressId = shippingAddressId;
                await _orderService.UpdateAsync(order);
            }

            var total = await _orderService.CalculateOrderTotalAsync(cart.Id);
            var paymentSuccess = await _paymentService.ProcessPaymentAsync(cart.Id, paymentMethod, total);

            if (paymentSuccess)
            {
                TempData["Success"] = "Order processed successfully!";
                return RedirectToAction(nameof(OrderConfirmation), new { id = cart.Id });
            }
            else
            {
                TempData["Error"] = "Error processing payment";
                return RedirectToAction(nameof(CheckoutAddressSelection));
            }
        }

        // GET: /customer/orders/confirmation/{id}
        [HttpGet("orders/confirmation/{id}")]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            int userId = GetCurrentUserId();
            var order = await _orderService.GetOrderWithDetailsAsync(id);

            if (order == null || order.UserId != userId)
            {
                TempData["Error"] = "Order not found";
                return RedirectToAction(nameof(Home));
            }

            return View(order);
        }

        // GET: /customer/orders
        [HttpGet("orders")]
        public async Task<IActionResult> MyOrders()
        {
            int userId = GetCurrentUserId();
            var orders = (await _orderService.GetByUserIdAsync(userId))
                .Where(o => o.Status != "Cart")
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var detailedOrders = new List<Order>();
            foreach (var o in orders)
            {
                var full = await _orderService.GetOrderWithDetailsAsync(o.Id);
                if (full != null)
                    detailedOrders.Add(full);
            }

            return View(detailedOrders);
        }

        // GET: /customer/orders/details/{id}
        [HttpGet("orders/details/{id}")]
        public async Task<IActionResult> OrderDetails(int id)
        {
            int userId = GetCurrentUserId();
            var order = await _orderService.GetOrderWithDetailsAsync(id);

            if (order == null || order.UserId != userId)
            {
                TempData["Error"] = "Order not found";
                return RedirectToAction(nameof(MyOrders));
            }

            return View(order);
        }

        // POST: /customer/orders/create
        [HttpPost("orders/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder()
        {
            int userId = GetCurrentUserId();
            var cart = await _orderService.GetActiveCartAsync(userId);

            if (cart == null)
            {
                TempData["Error"] = "Cart not found";
                return RedirectToAction(nameof(Cart));
            }

            cart = await _orderService.GetOrderWithDetailsAsync(cart.Id);

            if (cart.OrderItems == null || !cart.OrderItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            var subtotal = await _orderService.CalculateOrderTotalAsync(cart.Id);
            var tax = Math.Round(subtotal * 0.16m, 2);
            var shipping = 0m;
            var total = subtotal + tax + shipping;

            var newOrder = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                Total = total
            };

            var newOrderId = await _orderService.CreateAsync(newOrder);

            foreach (var oi in cart.OrderItems)
            {
                var newItem = new OrderItem
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                };
                await _orderService.AddItemToOrderAsync(newOrderId, newItem);
            }

            foreach (var oi in cart.OrderItems.ToList())
            {
                await _orderService.RemoveItemFromOrderAsync(oi.Id);
            }

            cart.Total = 0;
            await _orderService.UpdateAsync(cart);

            _logger.LogInformation("Order created {OrderId} for user {UserId}", newOrderId, userId);

            TempData["Success"] = "Order created successfully!";
            return RedirectToAction(nameof(OrderDetails), new { id = newOrderId });
        }

        // POST: /customer/orders/cancel/{id}
        [HttpPost("orders/cancel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            int userId = GetCurrentUserId();
            var order = await _orderService.GetByIdAsync(id);

            if (order == null || order.UserId != userId)
            {
                TempData["Error"] = "Order not found";
                return RedirectToAction(nameof(MyOrders));
            }

            if (order.Status != "Pending")
            {
                TempData["Error"] = "You can only cancel pending orders";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            var success = await _orderService.UpdateStatusAsync(id, "Cancelled");

            if (success)
                TempData["Success"] = "Order cancelled successfully!";
            else
                TempData["Error"] = "Error cancelling order";

            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        public class CreateOrderRequest
        {
            public bool Create { get; set; }
            public int? SelectedAddressId { get; set; }
        }

        #endregion

        #region Reviews

        [HttpGet("reviews/create/{productId}")]
        public async Task<IActionResult> CreateReview(int productId)
        {
            var product = await _productService.GetByIdAsync(productId);

            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction(nameof(Products));
            }

            TempData["Success"] = "Review created for " + product.Name;

            return RedirectToAction(nameof(ProductDetails), new { id = productId });
        }


        // POST: /customer/reviews/create
        [HttpPost("reviews/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(ReviewCreateViewModel model)
        {
            ModelState.Remove(nameof(model.ProductName));
            ModelState.Remove(nameof(model.ProductImageUrl));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = "Error: " + string.Join(", ", errors);

                return RedirectToAction(nameof(ProductDetails), new { id = model.ProductId });
            }

            var review = new Review
            {
                ProductId = model.ProductId,
                Rating = model.Rating,
                Comment = model.Comment,
                UserId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };

            await _reviewService.CreateAsync(review);

            TempData["Success"] = "Review created successfully!";

            return RedirectToAction(nameof(ProductDetails), new { id = model.ProductId });
        }

        // GET: /customer/reviews
        [HttpGet("reviews")]
        public async Task<IActionResult> MyReviews()
        {
            int userId = GetCurrentUserId();
            var reviews = await _reviewService.GetByUserIdAsync(userId);
            return View(reviews);
        }

        // POST: /customer/reviews/delete/{id}
        [HttpPost("reviews/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            int userId = GetCurrentUserId();
            var review = await _reviewService.GetByIdAsync(id);

            if (review == null || review.UserId != userId)
            {
                TempData["Error"] = "Review not found";
                return RedirectToAction(nameof(MyReviews));
            }

            var success = await _reviewService.DeleteAsync(id);

            if (success)
                TempData["Success"] = "Review deleted successfully!";
            else
                TempData["Error"] = "Error deleting review";

            return RedirectToAction(nameof(MyReviews));
        }

        #endregion

        #region Shipping Addresses

        // GET: /customer/addresses
        [HttpGet("addresses")]
        public async Task<IActionResult> Addresses()
        {
            int userId = GetCurrentUserId();
            var addresses = await _shippingAddressService.GetByUserIdAsync(userId);
            return View(addresses);
        }

        // GET: /customer/addresses/create
        [HttpGet("addresses/create")]
        public IActionResult CreateAddress()
        {
            return View();
        }

        // POST: /customer/addresses/create
        [HttpPost("addresses/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAddress(ShippingAddress address)
        {
            try
            {
                int userId = GetCurrentUserId();

                // Validar ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    var errorMessage = string.Join("; ", errors.Select(e => e.ErrorMessage));
                    
                    // Si es AJAX, devolver JSON
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, message = "Validation error: " + errorMessage });
                    
                    TempData["Error"] = "Por favor completa todos los campos requeridos";
                    return View(address);
                }

                // Asignar UserId
                address.UserId = userId;

                // Crear la dirección
                await _shippingAddressService.CreateAsync(address);

                TempData["Success"] = "Dirección creada exitosamente";
                return RedirectToAction(nameof(Addresses));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address for user");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Error: " + ex.Message });
                
                TempData["Error"] = "Error al crear la dirección: " + ex.Message;
                return View(address);
            }
        }

        // GET: /customer/addresses/edit/{id}
        [HttpGet("addresses/edit/{id}")]
        public async Task<IActionResult> EditAddress(int id)
        {
            int userId = GetCurrentUserId();
            var address = await _shippingAddressService.GetByIdAsync(id);

            if (address == null || address.UserId != userId)
            {
                TempData["Error"] = "Address not found";
                return RedirectToAction(nameof(Addresses));
            }

            return View(address);
        }

        // POST: /customer/addresses/edit/{id}
        [HttpPost("addresses/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(int id, ShippingAddress address)
        {
            int userId = GetCurrentUserId();
            var existingAddress = await _shippingAddressService.GetByIdAsync(id);

            if (existingAddress == null || existingAddress.UserId != userId)
            {
                TempData["Error"] = "Address not found";
                return RedirectToAction(nameof(Addresses));
            }

            if (!ModelState.IsValid)
                return View(address);

            address.Id = id;
            address.UserId = userId;
            var success = await _shippingAddressService.UpdateAsync(address);

            if (success)
                TempData["Success"] = "Address updated successfully!";
            else
                TempData["Error"] = "Error updating address";

            return RedirectToAction(nameof(Addresses));
        }

        // POST: /customer/addresses/delete/{id}
        [HttpPost("addresses/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            int userId = GetCurrentUserId();
            var address = await _shippingAddressService.GetByIdAsync(id);

            if (address == null || address.UserId != userId)
            {
                TempData["Error"] = "Address not found";
                return RedirectToAction(nameof(Addresses));
            }

            var success = await _shippingAddressService.DeleteAsync(id);

            if (success)
                TempData["Success"] = "Address deleted successfully!";
            else
                TempData["Error"] = "Error deleting address";

            return RedirectToAction(nameof(Addresses));
        }

        #endregion

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new Exception("Couldn't get current user ID. Be sure you're loged in.");

            if (int.TryParse(userIdClaim.Value, out int userId))
                return userId;

            throw new Exception("User ID it's not a valid format.");
        }

        #endregion

        #region Settings

        // GET: /customer/settings
        [HttpGet("settings")]
        public async Task<IActionResult> Settings()
        {
            int userId = GetCurrentUserId();
            var user = await _userService.GetByIdAsync(userId);

            if (user == null) return RedirectToAction("Login", "Auth");

            var model = new CustomerSettingsViewModel
            {
                Name = user.Name,
                Email = user.Email,
            };

            return View(model);
        }

        // POST: /customer/settings/profile
        [HttpPost("settings/profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(CustomerSettingsViewModel model)
        {
            ModelState.Remove("CurrentPassword");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmNewPassword");

            if (!ModelState.IsValid) return View("Settings", model);

            try
            {
                int userId = GetCurrentUserId();
                var user = await _userService.GetByIdAsync(userId);

                user.Name = model.Name;
                user.Email = model.Email;

                await _userService.UpdateAsync(user);

                TempData["Success"] = "Profile updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                TempData["Error"] = "An error occurred while updating the profile.";
            }

            return RedirectToAction(nameof(Settings));
        }

        [HttpPost("settings/security")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(CustomerSettingsViewModel model)
        {
            int userId = GetCurrentUserId();
            var user = await _userService.GetByIdAsync(userId);

            model.Name = user.Name;
            model.Email = user.Email;

            if (string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword))
            {
                TempData["Error"] = "All fields are required.";
                return View("Settings", model);
            }

            bool isCurrentPasswordCorrect = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);

            if (!isCurrentPasswordCorrect)
            {
                TempData["Error"] = "Current password is incorrect.";
                return View("Settings", model);
            }

            var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            user.PasswordHash = newHashedPassword;
            await _userService.UpdateAsync(user);

            TempData["Success"] = "Password updated successfully!";
            return RedirectToAction(nameof(Settings));
        }

        // POST: /customer/settings/delete-account
        [HttpPost("settings/delete-account")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                int userId = GetCurrentUserId();

                await _userService.DeleteAsync(userId);

                await HttpContext.SignOutAsync();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting account. Please contact support.";
                return RedirectToAction(nameof(Settings));
            }
        }

        #endregion
    }
}