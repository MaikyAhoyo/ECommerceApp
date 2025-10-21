using ECommerce.Models.Entities;

namespace ECommerce.Models.ViewModels
{
    public class CartViewModel
    {
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);

        public int Count => Items.Count;
    }
}
