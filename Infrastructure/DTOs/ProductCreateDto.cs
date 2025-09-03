using System.ComponentModel.DataAnnotations;

namespace TheStoreAPI.Infrastructure.DTOs
{
    public class ProductCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? ColorId { get; set; }

        public int? MaterialId { get; set; }

        public int? BrandId { get; set; }

        public string MainImageUrl { get; set; }

        [Required]
        public ICollection<ProductSizeCreateDto> Sizes { get; set; }

        public ICollection<string> OtherImageUrls { get; set; }

        public ICollection<ProductAttributeCreateDto> Attributes { get; set; }
    }
}
