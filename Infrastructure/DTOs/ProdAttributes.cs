using System.ComponentModel.DataAnnotations;

namespace TheStoreAPI.Infrastructure.DTOs
{
    
    public class ProductSizeCreateDto
    {
        [Required]
        public int SizeId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ProductAttributeDto
    {
        public string AttributeName { get; set; }
        public string Value { get; set; }
    }

    public class ProductAttributeCreateDto
    {
        [Required]
        public int AttributeId { get; set; }

        [Required]
        public string Value { get; set; }
    }
}