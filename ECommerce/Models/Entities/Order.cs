namespace ECommerce.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending/Confirmed/Shipped/Delivered/Cancelled
        public decimal Total { get; set; }
    }
}
