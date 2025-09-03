using System.ComponentModel.DataAnnotations;

namespace TheStoreAPI.Infrastructure.DTOs
{
    public class OrderItemCreateDto
    {
        [Required]
        public string TrackingId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
