using System;
using System.Collections.Generic;

namespace ECommerce.Models.Entities
{
    public class User
    {
        public int Id { get; set; } // User ID
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "Customer"; // Customer, Vendor, Admin
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();
        public ICollection<Product> Products { get; set; } = new List<Product>(); // As Vendor
    }
}
