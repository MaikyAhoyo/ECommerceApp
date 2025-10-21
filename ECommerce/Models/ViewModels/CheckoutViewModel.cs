using ECommerce.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public ShippingAddress ShippingAddress { get; set; } = new ShippingAddress();

        [Required]
        public Payment Payment { get; set; } = new Payment();

        [Required]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
    }
}
