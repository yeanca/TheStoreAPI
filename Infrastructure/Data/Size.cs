namespace TheStoreAPI.Infrastructure.Data
{
    public class Size
    {
        public int Id { get; set; }
        public int SizeCode { get; set; }
        public string Name { get; set; }
        public ICollection<ProductSize> ProductSizes { get; set; }
    }
}
