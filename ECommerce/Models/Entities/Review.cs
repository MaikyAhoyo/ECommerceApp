using System;

namespace ECommerce.Models.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; } // 1–5
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
