using ECommerce.Models.Entities;

namespace ECommerce.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = new Product();
        public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
    }
}
