using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;
using ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;


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
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            IProductService productService,
            IOrderService orderService,
            IReviewService reviewService,
            IShippingAddressService shippingAddressService,
            IPaymentService paymentService,
            ICategoryService categoryService,
            ILogger<CustomerController> logger)
        {
            _productService = productService;
            _orderService = orderService;
            _reviewService = reviewService;
            _shippingAddressService = shippingAddressService;
            _paymentService = paymentService;
            _categoryService = categoryService;
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
            var productCategories = await _categoryService.GetByProductIdAsync(product.Id);
            product.CategoryNames = string.Join(", ", productCategories.Select(c => ((Category)c).Name));

            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction(nameof(Products));
            }

            var reviews = await _reviewService.GetByProductIdAsync(id);
            var avgRating = await _reviewService.GetAverageRatingByProductAsync(id);
            var reviewCount = await _reviewService.GetReviewCountByProductAsync(id);
            var categories = await _productService.GetProductCategoriesAsync(id);
            var relatedProducts = (await _productService.GetAllAsync())
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToList();

            var viewModel = new CustomerProductDetailsViewModel
            {
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
                // Crear un carrito vacío
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

            // Mapear Order -> CustomerCartViewModel
            var viewModel = new CustomerCartViewModel();
            if (cart != null)
            {
                viewModel.Cart = cart;

                // Map OrderItems to CartItemViewModel
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

                // Calcular totales
                viewModel.Subtotal = await _orderService.CalculateOrderTotalAsync(cart.Id);
                viewModel.Tax = Math.Round(viewModel.Subtotal * 0.16m, 2);
                viewModel.Shipping = 0m; // lógica de envío simple
                viewModel.Total = viewModel.Subtotal + viewModel.Tax + viewModel.Shipping;
                viewModel.ItemCount = viewModel.Items.Sum(i => i.Quantity);
            }

            return View(viewModel);
        }

        // POST: /customer/cart/add
        [HttpPost("cart/add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            int productId = request?.ProductId ?? 0;
            int quantity = request?.Quantity ?? 1;

            int userId = GetCurrentUserId();

            // 1. Obtener el producto para saber el Stock real
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Producto no encontrado" });
            }

            // 2. Obtener el carrito activo para ver qué tenemos ya guardado
            var cart = await _orderService.GetActiveCartAsync(userId);

            int currentQuantityInCart = 0;

            if (cart != null)
            {
                // Usamos el método GetOrderItemsAsync que ya existe en tu OrderService
                var cartItems = await _orderService.GetOrderItemsAsync(cart.Id);
                var existingItem = cartItems.FirstOrDefault(i => i.ProductId == productId);

                if (existingItem != null)
                {
                    currentQuantityInCart = existingItem.Quantity;
                }
            }

            // 3. VALIDACIÓN CORREGIDA: (Lo que ya tengo + lo que quiero agregar) vs Stock
            if (currentQuantityInCart + quantity > product.Stock)
            {
                return Json(new
                {
                    success = false,
                    message = $"Stock insuficiente. Solo hay {product.Stock} disponibles."
                });
            }

            // --- A partir de aquí el código sigue igual que antes, pero debes mover la lógica de creación del carrito ---

            // Si el carrito es null, lo creamos ahora (ya sabemos que el stock es válido)
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

            // Agregar item al carrito
            var item = new OrderItem
            {
                ProductId = productId,
                Quantity = quantity, // Aquí mandamos solo la cantidad nueva, tu OrderService ya hace la suma en SQL
                UnitPrice = product.Price
            };

            var success = await _orderService.AddItemToOrderAsync(cart.Id, item);

            if (success)
            {
                var total = await _orderService.CalculateOrderTotalAsync(cart.Id);
                cart.Total = total;
                await _orderService.UpdateAsync(cart);

                return Json(new { success = true, message = "Producto agregado al carrito" });
            }

            return Json(new { success = false, message = "Error al agregar al carrito" });
        }
        public class AddToCartRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        // POST: /customer/cart/update/{itemId}
        [HttpPost("cart/update/{itemId}")]
        public async Task<IActionResult> UpdateCartItem(int itemId, int quantity)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Cantidad inválida" });
            }

            try
            {
                int userId = GetCurrentUserId();
                var cart = await _orderService.GetActiveCartAsync(userId);
                if (cart == null)
                {
                    return Json(new { success = false, message = "Carrito no encontrado" });
                }

                var cartItems = await _orderService.GetOrderItemsAsync(cart.Id);
                var itemToUpdate = cartItems.FirstOrDefault(i => i.Id == itemId);


                if (itemToUpdate == null)
                {
                    return Json(new { success = false, message = "El producto no se encuentra en tu carrito" });
                }

                if (itemToUpdate.Product != null && quantity > itemToUpdate.Product.Stock)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Stock insuficiente. Solo hay {itemToUpdate.Product.Stock} unidades disponibles."
                    });
                }

                var success = await _orderService.UpdateOrderItemQuantityAsync(itemId, quantity);

                if (success)
                    return Json(new { success = true, message = "Cantidad actualizada" });
                else
                    return Json(new { success = false, message = "Error al actualizar" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: /customer/cart/remove/{itemId}
        [HttpPost("cart/remove/{itemId}")]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            var success = await _orderService.RemoveItemFromOrderAsync(itemId);

            if (success)
            {
                TempData["Success"] = "Product removed from cart";
            }
            else
            {
                TempData["Error"] = "Error removing product";
            }

            return RedirectToAction(nameof(Cart));
        }

        #endregion

        #region Checkout & Orders

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

            // Build view model expected by the view
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

            // Checkout
            var success = await _orderService.CheckoutOrderAsync(cart.Id);

            if (!success)
            {
                TempData["Error"] = "Error processing order";
                return RedirectToAction(nameof(Checkout));
            }

            // Process payment
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
                return RedirectToAction(nameof(Checkout));
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
        }

        // POST: /customer/orders/create
        [HttpPost("orders/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest? request)
        {
            // If request is null, try to read from form values (e.g., from FormData)
            if (request == null && Request.HasFormContentType)
            {
                var createVal = Request.Form["create"].FirstOrDefault();
                if (bool.TryParse(createVal, out var parsed))
                {
                    request = new CreateOrderRequest { Create = parsed };
                }
            }

            int userId = GetCurrentUserId();
            var cart = await _orderService.GetActiveCartAsync(userId);

            if (cart == null)
            {
                TempData["Error"] = "Cart not found";
                var isAjaxNull = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                                (Request.Headers.ContainsKey("Accept") && Request.Headers["Accept"].ToString().Contains("application/json"));
                if (isAjaxNull)
                    return Json(new { success = false, message = "Cart not found" });

                return RedirectToAction(nameof(Cart));
            }

            // Ensure we have the order details (items)
            cart = await _orderService.GetOrderWithDetailsAsync(cart.Id);

            if (cart.OrderItems == null || !cart.OrderItems.Any())
            {
                TempData["Error"] = "Cart is empty";
                var isAjaxEmpty = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                                (Request.Headers.ContainsKey("Accept") && Request.Headers["Accept"].ToString().Contains("application/json"));
                if (isAjaxEmpty)
                    return Json(new { success = false, message = "Cart is empty" });

                return RedirectToAction(nameof(Cart));
            }

            // Build items info for logging/response
            var itemsInfo = cart.OrderItems.Select(oi => new
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "(no name)",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Subtotal = oi.Quantity * oi.UnitPrice
            }).ToList();

            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                         (Request.Headers.ContainsKey("Accept") && Request.Headers["Accept"].ToString().Contains("application/json"));

            // If AJAX and request doesn't ask to create, just return items
            if (isAjax && (request == null || request.Create == false))
            {
                _logger.LogInformation("(AJAX) Returning cart items for user {UserId}. Items: {Items}", userId, System.Text.Json.JsonSerializer.Serialize(itemsInfo));
                return Json(new { success = true, items = itemsInfo });
            }

            // Proceed to create a new Order (copy items) so the cart can be reused later
            // Calculate subtotal, tax and total
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

            // Copy each item into the new order
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

            // Recalculate new order total to ensure consistency
            var newSubtotal = await _orderService.CalculateOrderTotalAsync(newOrderId);
            var newTax = Math.Round(newSubtotal * 0.16m, 2);
            var newTotal = newSubtotal + newTax + shipping;

            var createdOrder = await _orderService.GetByIdAsync(newOrderId);
            if (createdOrder != null)
            {
                createdOrder.Total = newTotal;
                await _orderService.UpdateAsync(createdOrder);
            }

            // Clear items from the cart so user can create more orders later
            foreach (var oi in cart.OrderItems.ToList())
            {
                await _orderService.RemoveItemFromOrderAsync(oi.Id);
            }

            // Ensure cart total reset
            cart.Total = 0;
            await _orderService.UpdateAsync(cart);

            _logger.LogInformation("Created new order {OrderId} for user {UserId}. Items copied: {Count}", newOrderId, userId, itemsInfo.Count);

            if (isAjax)
            {
                return Json(new { success = true, message = "Order created", orderId = newOrderId, items = itemsInfo, total = newTotal, tax = newTax });
            }

            TempData["Success"] = "Order created (test mode)";
            return RedirectToAction(nameof(OrderDetails), new { id = newOrderId });
        }

        #endregion

        #region Reviews

        // GET: /customer/reviews/create/{productId}
        [HttpGet("reviews/create/{productId}")]
        public async Task<IActionResult> CreateReview(int productId)
        {
            var product = await _productService.GetByIdAsync(productId);

            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Product = product;
            return View();
        }

        // POST: /customer/reviews/create
        [HttpPost("reviews/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(Review review)
        {
            if (!ModelState.IsValid)
            {
                var product = await _productService.GetByIdAsync(review.ProductId);
                ViewBag.Product = product;
                return View(review);
            }

            review.UserId = GetCurrentUserId();
            review.CreatedAt = DateTime.UtcNow;

            await _reviewService.CreateAsync(review);
            TempData["Success"] = "Review created successfully!";

            return RedirectToAction(nameof(ProductDetails), new { id = review.ProductId });
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
    }
}