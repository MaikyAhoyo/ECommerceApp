namespace ECommerce.Models.Entities
{
    public class ShippingAddress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string PostalCode { get; set; } = null!;

        // Relationships
        public User User { get; set; } = null!;
    }
}
