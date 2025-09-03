using System.ComponentModel.DataAnnotations;

namespace TheStoreAPI.Infrastructure.DTOs
{
    public class OrderCreateDto
    {
        [Required]
        public ICollection<OrderItemCreateDto> Items { get; set; }
    }
}
