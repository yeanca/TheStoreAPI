namespace TheStoreAPI.Infrastructure.Data
{
    public class ProductSize
    {
        public int itemSizeId { get; set; }
        public string? TrackingId { get; set; }
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Product Product { get; set; }
        public Size Size { get; set; }
    }
}
