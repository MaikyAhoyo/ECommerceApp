using ECommerce.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.ViewModels
{
    // ========================================
    // ADMIN VIEW MODELS
    // ========================================

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

    public class AdminSalesReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
        public Dictionary<string, decimal> RevenueByStatus { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
    }

    public class AdminUserDetailsViewModel
    {
        public User User { get; set; } = null!;
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Product> Products { get; set; } = new List<Product>(); // Si es vendor
        public List<Review> Reviews { get; set; } = new List<Review>();
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    // ========================================
    // VENDOR VIEW MODELS
    // ========================================

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
        public decimal MonthlyRevenue { get; set; }
        public int MonthlyOrders { get; set; }
    }

    public class VendorInventoryViewModel
    {
        public List<Product> AllProducts { get; set; } = new List<Product>();
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
        public List<Product> OutOfStockProducts { get; set; } = new List<Product>();
        public int TotalProducts { get; set; }
        public int TotalStock { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class VendorSalesReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProductsSold { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
        public Dictionary<string, int> ProductsSoldByMonth { get; set; } = new Dictionary<string, int>();
        public List<ProductSalesData> TopSellingProducts { get; set; } = new List<ProductSalesData>();
    }

    public class VendorProductDetailsViewModel
    {
        public Product Product { get; set; } = null!;
        public List<Review> Reviews { get; set; } = new List<Review>();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // ========================================
    // CUSTOMER VIEW MODELS
    // ========================================

    public class CustomerHomeViewModel
    {
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Product> NewArrivals { get; set; } = new List<Product>();
        public List<Product> BestSellers { get; set; } = new List<Product>();
        public List<Product> GoldProducts { get; set; } = new List<Product>();
        public List<Product> SilverProducts { get; set; } = new List<Product>();
    }

    public class CustomerProductsViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public string? CurrentMetal { get; set; }
        public int? CurrentCategory { get; set; }
        public string? SearchTerm { get; set; }
        public string? CurrentSearch { get; set; }
        public int? CurrentCategoryId { get; set; }
        public decimal? CurrentMinPrice { get; set; }
        public decimal? CurrentMaxPrice { get; set; }
        public string? CurrentSortBy { get; set; }
        public int TotalProducts { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; } = 1;
    }

    public class CustomerProductDetailsViewModel
    {
        public Product Product { get; set; } = null!;
        public List<Review> Reviews { get; set; } = new List<Review>();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Product> RelatedProducts { get; set; } = new List<Product>();
        public bool InStock { get; set; }
    }

    public class CustomerCartViewModel
    {
        public Order Cart { get; set; } = null!;
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }
        public int ItemCount { get; set; }
    }

    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductImageUrl { get; set; } = null!;
        public string Metal { get; set; } = null!;
        public decimal Purity { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
        public int MaxStock { get; set; }
    }

    public class CustomerCheckoutViewModel
    {
        public Order Cart { get; set; } = null!;
        public List<ShippingAddress> Addresses { get; set; } = new List<ShippingAddress>();
        public int? SelectedAddressId { get; set; }
        public string? SelectedPaymentMethod { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }
    }

    public class CustomerOrderHistoryViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public string? StatusFilter { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class CustomerOrderDetailsViewModel
    {
        public Order Order { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }
        public ShippingAddress? ShippingAddress { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReview { get; set; }
    }

    // ========================================
    // AUTH VIEW MODELS
    // ========================================

    public class LoginViewModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Debes confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = null!;

        [Display(Name = "Registrarse como vendedor")]
        public bool RegisterAsVendor { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Debes confirmar la nueva contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmNewPassword { get; set; } = null!;
    }

    public class UserProfileViewModel
    {
        public User User { get; set; } = null!;
        public int TotalOrders { get; set; }
        public int TotalReviews { get; set; }
        public List<ShippingAddress> Addresses { get; set; } = new List<ShippingAddress>();

        // Solo para vendedores
        public int? TotalProducts { get; set; }
        public decimal? TotalRevenue { get; set; }
    }

    // ========================================
    // PRODUCT VIEW MODELS
    // ========================================

    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        [Display(Name = "Nombre del producto")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es requerida")]
        [Display(Name = "Descripción")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "El stock es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Display(Name = "URL de la imagen")]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "El metal es requerido")]
        [Display(Name = "Metal")]
        public string Metal { get; set; } = null!; // Gold, Silver, Platinum

        [Required(ErrorMessage = "La pureza es requerida")]
        [Range(0.001, 999.999, ErrorMessage = "Pureza inválida")]
        [Display(Name = "Pureza")]
        public decimal Purity { get; set; }

        [Display(Name = "Categorías")]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();

        // Para llenar en el controlador
        public List<Category> AvailableCategories { get; set; } = new List<Category>();
        public List<User> AvailableVendors { get; set; } = new List<User>(); // Solo para admin
    }

    public class ProductEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200)]
        [Display(Name = "Nombre del producto")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es requerida")]
        [Display(Name = "Descripción")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Precio")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "El stock es requerido")]
        [Range(0, int.MaxValue)]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Display(Name = "URL de la imagen")]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "El metal es requerido")]
        [Display(Name = "Metal")]
        public string Metal { get; set; } = null!;

        [Required(ErrorMessage = "La pureza es requerida")]
        [Range(0.001, 999.999)]
        [Display(Name = "Pureza")]
        public decimal Purity { get; set; }

        [Display(Name = "Categorías")]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();

        public List<Category> AvailableCategories { get; set; } = new List<Category>();
        public List<Category> CurrentCategories { get; set; } = new List<Category>();
    }

    // ========================================
    // REVIEW VIEW MODELS
    // ========================================

    public class ReviewCreateViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductImageUrl { get; set; } = null!;

        [Required(ErrorMessage = "La calificación es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe ser entre 1 y 5")]
        [Display(Name = "Calificación")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "El comentario no puede exceder 1000 caracteres")]
        [Display(Name = "Comentario (opcional)")]
        public string? Comment { get; set; }
    }

    public class ReviewEditViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;

        [Required(ErrorMessage = "La calificación es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe ser entre 1 y 5")]
        [Display(Name = "Calificación")]
        public int Rating { get; set; }

        [StringLength(1000)]
        [Display(Name = "Comentario")]
        public string? Comment { get; set; }
    }

    // ========================================
    // SHARED/COMMON VIEW MODELS
    // ========================================

    public class ProductSalesData
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class OrderStatusUpdateViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        public string Status { get; set; } = null!;

        public DateTime? ArrivalDate { get; set; }
    }

    public class StockUpdateViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        public int Quantity { get; set; }
    }

    // ========================================
    // PAGINACIÓN
    // ========================================

    public class PaginatedList<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }

    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public string? Action { get; set; }
        public string? Controller { get; set; }
        public Dictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>();
    }
}