namespace TheStoreAPI.Infrastructure.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string MainImageUrl { get; set; }
        public int TotalStockQuantity { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string ColorName { get; set; }
        public string MaterialName { get; set; }

        public ICollection<ProductSizeDto> Sizes { get; set; }
        public ICollection<ProductImageDto> OtherImages { get; set; }
        public ICollection<ProductAttributeDto> Attributes { get; set; }
    }
}