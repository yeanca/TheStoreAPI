

namespace TheStoreAPI.Infrastructure.Data
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int? ColorId { get; set; } 
        public int? MaterialId { get; set; } 
        public int? BrandId { get; set; }
        public int TotalStockQuantity { get; set; }
        public bool IsActive { get; set; }
        public bool IsHot { get; set; }
        public bool IsDeleted { get; set; }
        public string MainImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Category Category { get; set; }
        public Color Color { get; set; } // New navigation property
        public Material Material { get; set; } // New navigation property
        public Brand Brand { get; set; } // New navigation property
        public ICollection<ProductSize> ProductSizes { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; }
        public ICollection<ProductAttribute> ProductAttributes { get; set; }
    }
}
