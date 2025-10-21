using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
    public class ReviewViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(500)]
        public string Comment { get; set; } = string.Empty;
    }
}
