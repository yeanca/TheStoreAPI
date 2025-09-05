namespace TheStoreAPI.Infrastructure.DTOs
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public string TrackingId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ProductName { get; set; }
    }
}
