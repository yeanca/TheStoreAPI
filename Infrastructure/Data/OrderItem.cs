namespace TheStoreAPI.Infrastructure.Data
{
    public class OrderItem
    {
        public int Id { get; set; }
        public long OrderId { get; set; }
        public string TrackingId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Order Order { get; set; }
        public ProductSize ProductSize { get; set; }
    }
}
