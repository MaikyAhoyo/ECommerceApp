using ECommerce.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.ViewModels
{
    #region Admin View Models
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
    #endregion

    #region Vendor View Models
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
    #endregion

    #region Customer View Models
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

    public class CheckoutAddressSelectionViewModel
    {
        public Order Cart { get; set; } = null!;
        public List<ShippingAddress> Addresses { get; set; } = new List<ShippingAddress>();
        public int? SelectedAddressId { get; set; }
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

    

    #endregion

    #region Auth View Models
    // ========================================
    // AUTH VIEW MODELS
    // ========================================

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [Display(Name = "Remembre Me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Password confirmation is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;

        [Display(Name = "Register as Vendor")]
        public bool RegisterAsVendor { get; set; }

        public string? Role { get; internal set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your new password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; } = null!;
    }

    public class UserProfileViewModel
    {
        public User User { get; set; } = null!;
        public int TotalOrders { get; set; }
        public int TotalReviews { get; set; }
        public List<ShippingAddress> Addresses { get; set; } = new List<ShippingAddress>();
        public int? TotalProducts { get; set; }
        public decimal? TotalRevenue { get; set; }
    }
    #endregion

    #region Product View Models
    // ========================================
    // PRODUCT VIEW MODELS
    // ========================================

    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Display(Name = "Image URL")]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Metal type is required")]
        [Display(Name = "Metal")]
        public string Metal { get; set; } = null!; // Gold, Silver, Platinum

        [Required(ErrorMessage = "Purity is required")]
        [Range(0.001, 999.999, ErrorMessage = "Invalid purity value")]
        [Display(Name = "Purity")]
        public decimal Purity { get; set; }

        [Display(Name = "Categories")]
        public List<int> SelectedCategoryIds { get; set; } = new();

        public List<Category> AvailableCategories { get; set; } = new();
    }

    public class ProductEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock is required")]
        [Range(0, int.MaxValue)]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Display(Name = "Image URL")]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Metal type is required")]
        [Display(Name = "Metal")]
        public string Metal { get; set; } = null!;

        [Required(ErrorMessage = "Purity is required")]
        [Range(0.001, 999.999)]
        [Display(Name = "Purity")]
        public decimal Purity { get; set; }

        [Display(Name = "Categories")]
        public List<int> SelectedCategoryIds { get; set; } = new();

        public List<Category> AvailableCategories { get; set; } = new();
        public List<Category> CurrentCategories { get; set; } = new();
    }
    #endregion

    #region Review View Models
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
    #endregion

    #region Shared/Common View Models
    // ========================================
    // SHARED/COMMON VIEW MODELS
    // ========================================

    public class ProductSalesData
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal TotalRevenue { get; internal set; }
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
    #endregion

    #region Pagination
    // ========================================
    // PAGINATION
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
    #endregion
}