using System;

namespace ECommerce.Models.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string Method { get; set; } = "Card"; // Card, PayPal, Transfer
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed

        // Relationships
        public Order Order { get; set; } = null!;
    }
}
