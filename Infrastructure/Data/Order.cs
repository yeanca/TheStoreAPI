namespace TheStoreAPI.Infrastructure.Data
{
    public class Order
    {
        public long Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string AnonymousUserId { get; set; }
        public decimal TotalPrice { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
