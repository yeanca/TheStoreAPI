using System.ComponentModel.DataAnnotations;

namespace TheStoreAPI.Infrastructure.DTOs
{
    public class ProductUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Price { get; set; }

        public int? CategoryId { get; set; }

        public int? ColorId { get; set; }

        public int? MaterialId { get; set; }

        public int? BrandId { get; set; }

        public string MainImageUrl { get; set; }

        public bool? IsActive { get; set; }
        public bool? IsHot { get; set; }
    }
}
