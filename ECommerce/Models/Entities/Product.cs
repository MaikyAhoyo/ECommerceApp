using System.Collections.Generic;

namespace ECommerce.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Discount { get; set; }
        public int Stock { get; set; }
        public int VendorId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string Metal { get; set; } = null!; // Gold / Silver / Platinum
        public decimal Purity { get; set; } // e.g., 14, 18, 24, 925, 950

        // Relationships
        public User Vendor { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public string CategoryNames { get; set; } = null!;
    }
}
