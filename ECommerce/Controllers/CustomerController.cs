using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;
using ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
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

        public CustomerController(
            IProductService productService,
            IOrderService orderService,
            IReviewService reviewService,
            IShippingAddressService shippingAddressService,
            IPaymentService paymentService,
            ICategoryService categoryService)
        {
            _productService = productService;
            _orderService = orderService;
            _reviewService = reviewService;
            _shippingAddressService = shippingAddressService;
            _paymentService = paymentService;
            _categoryService = categoryService;
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
        int page = 1)
        {
            var products = (await _productService.GetAllAsync()).ToList();

            foreach (var p in products)
            {
                var productCategories = await _categoryService.GetByProductIdAsync(p.Id);
                p.CategoryNames = string.Join(", ", productCategories.Select(c => ((Category)c).Name));
            }

            if (!string.IsNullOrEmpty(search))
                products = products
                    .Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(metal))
                products = products
                    .Where(p => p.Metal.Equals(metal, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (categoryId.HasValue)
                products = products
                    .Where(p => p.CategoryId == categoryId.Value)
                    .ToList();

            if (minPrice.HasValue)
                products = products
                    .Where(p => p.Price >= minPrice.Value)
                    .ToList();

            if (maxPrice.HasValue)
                products = products
                    .Where(p => p.Price <= maxPrice.Value)
                    .ToList();

            products = sortBy switch
            {
                "price_asc" => products.OrderBy(p => p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                "name_asc" => products.OrderBy(p => p.Name).ToList(),
                "name_desc" => products.OrderByDescending(p => p.Name).ToList(),
                _ => products
            };

            const int pageSize = 12;
            int totalProducts = products.Count;
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var paginatedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var categories = (await _categoryService.GetAllAsync()).ToList();

            var viewModel = new CustomerProductsViewModel
            {
                Products = paginatedProducts,
                Categories = categories,
                CurrentMetal = metal,
                CurrentCategoryId = categoryId,
                CurrentMinPrice = minPrice,
                CurrentMaxPrice = maxPrice,
                CurrentSortBy = sortBy,
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
                TempData["Error"] = "Producto no encontrado";
                return RedirectToAction(nameof(Products));
            }

            var reviews = await _reviewService.GetByProductIdAsync(id);
            var avgRating = await _reviewService.GetAverageRatingByProductAsync(id);
            var reviewCount = await _reviewService.GetReviewCountByProductAsync(id);
            var categories = await _productService.GetProductCategoriesAsync(id);

            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = avgRating;
            ViewBag.ReviewCount = reviewCount;
            ViewBag.Categories = categories;

            return View(product);
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

            return View(cart);
        }

        // POST: /customer/cart/add
        [HttpPost("cart/add")]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            int userId = GetCurrentUserId();

            // Verificar que el producto existe y tiene stock
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Producto no encontrado" });
            }

            if (product.Stock < quantity)
            {
                return Json(new { success = false, message = "Stock insuficiente" });
            }

            // Obtener o crear carrito
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
                cart = await _orderService.GetByIdAsync(cartId);
            }

            // Agregar item al carrito
            var item = new OrderItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price
            };

            var success = await _orderService.AddItemToOrderAsync(cart.Id, item);

            if (success)
            {
                // Recalcular total
                var total = await _orderService.CalculateOrderTotalAsync(cart.Id);
                cart.Total = total;
                await _orderService.UpdateAsync(cart);

                return Json(new { success = true, message = "Producto agregado al carrito" });
            }

            return Json(new { success = false, message = "Error al agregar al carrito" });
        }

        // POST: /customer/cart/update/{itemId}
        [HttpPost("cart/update/{itemId}")]
        public async Task<IActionResult> UpdateCartItem(int itemId, int quantity)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Cantidad inválida" });
            }

            var success = await _orderService.UpdateOrderItemQuantityAsync(itemId, quantity);

            if (success)
                return Json(new { success = true, message = "Cantidad actualizada" });
            else
                return Json(new { success = false, message = "Error al actualizar" });
        }

        // POST: /customer/cart/remove/{itemId}
        [HttpPost("cart/remove/{itemId}")]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            var success = await _orderService.RemoveItemFromOrderAsync(itemId);

            if (success)
            {
                TempData["Success"] = "Producto eliminado del carrito";
            }
            else
            {
                TempData["Error"] = "Error al eliminar el producto";
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
                TempData["Error"] = "Tu carrito está vacío";
                return RedirectToAction(nameof(Cart));
            }

            cart = await _orderService.GetOrderWithDetailsAsync(cart.Id);

            if (cart.OrderItems == null || !cart.OrderItems.Any())
            {
                TempData["Error"] = "Tu carrito está vacío";
                return RedirectToAction(nameof(Cart));
            }

            var addresses = await _shippingAddressService.GetByUserIdAsync(userId);
            ViewBag.Addresses = addresses;

            return View(cart);
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
                TempData["Error"] = "Carrito no encontrado";
                return RedirectToAction(nameof(Cart));
            }

            // Realizar checkout
            var success = await _orderService.CheckoutOrderAsync(cart.Id);

            if (!success)
            {
                TempData["Error"] = "Error al procesar la orden";
                return RedirectToAction(nameof(Checkout));
            }

            // Procesar pago
            var total = await _orderService.CalculateOrderTotalAsync(cart.Id);
            var paymentSuccess = await _paymentService.ProcessPaymentAsync(cart.Id, paymentMethod, total);

            if (paymentSuccess)
            {
                TempData["Success"] = "¡Orden procesada exitosamente!";
                return RedirectToAction(nameof(OrderConfirmation), new { id = cart.Id });
            }
            else
            {
                TempData["Error"] = "Error al procesar el pago";
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
                TempData["Error"] = "Orden no encontrada";
                return RedirectToAction(nameof(Home));
            }

            return View(order);
        }

        // GET: /customer/orders
        [HttpGet("orders")]
        public async Task<IActionResult> MyOrders()
        {
            int userId = GetCurrentUserId();
            var orders = await _orderService.GetByUserIdAsync(userId);

            // Filtrar el carrito activo
            orders = orders.Where(o => o.Status != "Cart");

            return View(orders.OrderByDescending(o => o.OrderDate));
        }

        // GET: /customer/orders/details/{id}
        [HttpGet("orders/details/{id}")]
        public async Task<IActionResult> OrderDetails(int id)
        {
            int userId = GetCurrentUserId();
            var order = await _orderService.GetOrderWithDetailsAsync(id);

            if (order == null || order.UserId != userId)
            {
                TempData["Error"] = "Orden no encontrada";
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
                TempData["Error"] = "Orden no encontrada";
                return RedirectToAction(nameof(MyOrders));
            }

            if (order.Status != "Pending")
            {
                TempData["Error"] = "Solo puedes cancelar órdenes pendientes";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            var success = await _orderService.UpdateStatusAsync(id, "Cancelled");

            if (success)
                TempData["Success"] = "Orden cancelada exitosamente";
            else
                TempData["Error"] = "Error al cancelar la orden";

            return RedirectToAction(nameof(OrderDetails), new { id });
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
                TempData["Error"] = "Producto no encontrado";
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
            TempData["Success"] = "Reseña creada exitosamente";

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

        // GET: /customer/reviews/edit/{id}
        [HttpGet("reviews/edit/{id}")]
        public async Task<IActionResult> EditReview(int id)
        {
            int userId = GetCurrentUserId();
            var review = await _reviewService.GetByIdAsync(id);

            if (review == null || review.UserId != userId)
            {
                TempData["Error"] = "Reseña no encontrada";
                return RedirectToAction(nameof(MyReviews));
            }

            var product = await _productService.GetByIdAsync(review.ProductId);
            ViewBag.Product = product;

            return View(review);
        }

        // POST: /customer/reviews/edit/{id}
        [HttpPost("reviews/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(int id, Review review)
        {
            int userId = GetCurrentUserId();
            var existingReview = await _reviewService.GetByIdAsync(id);

            if (existingReview == null || existingReview.UserId != userId)
            {
                TempData["Error"] = "Reseña no encontrada";
                return RedirectToAction(nameof(MyReviews));
            }

            if (!ModelState.IsValid)
            {
                var product = await _productService.GetByIdAsync(review.ProductId);
                ViewBag.Product = product;
                return View(review);
            }

            review.Id = id;
            var success = await _reviewService.UpdateAsync(review);

            if (success)
                TempData["Success"] = "Reseña actualizada exitosamente";
            else
                TempData["Error"] = "Error al actualizar la reseña";

            return RedirectToAction(nameof(MyReviews));
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
                TempData["Error"] = "Reseña no encontrada";
                return RedirectToAction(nameof(MyReviews));
            }

            var success = await _reviewService.DeleteAsync(id);

            if (success)
                TempData["Success"] = "Reseña eliminada exitosamente";
            else
                TempData["Error"] = "Error al eliminar la reseña";

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
            if (!ModelState.IsValid)
                return View(address);

            address.UserId = GetCurrentUserId();
            await _shippingAddressService.CreateAsync(address);

            TempData["Success"] = "Dirección creada exitosamente";
            return RedirectToAction(nameof(Addresses));
        }

        // GET: /customer/addresses/edit/{id}
        [HttpGet("addresses/edit/{id}")]
        public async Task<IActionResult> EditAddress(int id)
        {
            int userId = GetCurrentUserId();
            var address = await _shippingAddressService.GetByIdAsync(id);

            if (address == null || address.UserId != userId)
            {
                TempData["Error"] = "Dirección no encontrada";
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
                TempData["Error"] = "Dirección no encontrada";
                return RedirectToAction(nameof(Addresses));
            }

            if (!ModelState.IsValid)
                return View(address);

            address.Id = id;
            address.UserId = userId;
            var success = await _shippingAddressService.UpdateAsync(address);

            if (success)
                TempData["Success"] = "Dirección actualizada exitosamente";
            else
                TempData["Error"] = "Error al actualizar la dirección";

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
                TempData["Error"] = "Dirección no encontrada";
                return RedirectToAction(nameof(Addresses));
            }

            var success = await _shippingAddressService.DeleteAsync(id);

            if (success)
                TempData["Success"] = "Dirección eliminada exitosamente";
            else
                TempData["Error"] = "Error al eliminar la dirección";

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