using System;
using System.Collections.Generic;

namespace ECommerce.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AddressId { get; set; } = 1;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ArrivalDate { get; set; }
        public string Status { get; set; } = "Cart"; // Cart, Pending, Confirmed, Shipped, Delivered, Cancelled
        public decimal Total { get; set; }

        // Relationships
        public User User { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }
    }
}
