namespace TheStoreAPI.Infrastructure.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        Processing = 2,
        Packed = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        Returned = 7,
        Dispute = 8,
    }
}
