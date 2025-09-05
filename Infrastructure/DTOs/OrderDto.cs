namespace TheStoreAPI.Infrastructure.DTOs
{
    public class OrderDto
    {
        public long OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int Status { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}

