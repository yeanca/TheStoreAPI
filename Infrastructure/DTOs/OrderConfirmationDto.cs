namespace TheStoreAPI.Infrastructure.DTOs
{
    public class OrderConfirmationDto
    {
        public long OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
    }
}
