using Microsoft.AspNetCore.Mvc;

namespace TheStoreAPI.Infrastructure.Data
{
    public class Attribute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ProductAttribute> ProductAttributes { get; set; }
    }
}
