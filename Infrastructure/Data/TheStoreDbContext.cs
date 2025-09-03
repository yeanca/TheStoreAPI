using Microsoft.EntityFrameworkCore;

namespace TheStoreAPI.Infrastructure.Data
{
    public class TheStoreDbContext : DbContext
    {
        public TheStoreDbContext(DbContextOptions<TheStoreDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<Attribute> Attributes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Product entity and all its relationships
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.ColorId).IsRequired(false);
                entity.Property(e => e.MaterialId).IsRequired(false);
                entity.Property(e => e.BrandId).IsRequired(false);

                // Explicitly map all one-to-many relationships
                entity.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
                entity.HasOne(p => p.Brand).WithMany(b => b.Products).HasForeignKey(p => p.BrandId);
                entity.HasOne(p => p.Color).WithMany().HasForeignKey(p => p.ColorId);
                entity.HasOne(p => p.Material).WithMany().HasForeignKey(p => p.MaterialId);

                // Explicitly map all one-to-many relationships with collection navigation
                entity.HasMany(p => p.ProductImages).WithOne(pi => pi.Product).HasForeignKey(pi => pi.ProductId);
                entity.HasMany(p => p.ProductSizes).WithOne(ps => ps.Product).HasForeignKey(ps => ps.ProductId);
                entity.HasMany(p => p.ProductAttributes).WithOne(pa => pa.Product).HasForeignKey(pa => pa.ProductId);
            });

            // Configure ProductAttributes entity
            modelBuilder.Entity<ProductAttribute>(entity =>
            {
                entity.ToTable("ProductAttributes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(pa => pa.Product).WithMany(p => p.ProductAttributes).HasForeignKey(pa => pa.ProductId);
                entity.HasOne(pa => pa.Attribute).WithMany(a => a.ProductAttributes).HasForeignKey(pa => pa.AttributeId);
            });

            // Configure ProductImage entity
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            // Configure ProductSize entity
            modelBuilder.Entity<ProductSize>(entity =>
            {
                entity.ToTable("ProductSizes");
                entity.HasKey(e => e.itemSizeId);
                entity.Property(e => e.itemSizeId).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.TrackingId).IsUnique();
                entity.HasOne(ps => ps.Product).WithMany(p => p.ProductSizes).HasForeignKey(ps => ps.ProductId);
                entity.HasOne(ps => ps.Size).WithMany(s => s.ProductSizes).HasForeignKey(ps => ps.SizeId);
            });

            // Configure other entities
            modelBuilder.Entity<Category>().ToTable("Categories").HasKey(e => e.Id);
            modelBuilder.Entity<Category>().Property(e => e.Id).ValueGeneratedOnAdd();
            // REMOVED: Redundant relationship configuration
            // modelBuilder.Entity<Category>().HasMany<Product>().WithOne(p => p.Category).HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Brand>().ToTable("Brands").HasKey(e => e.Id);
            modelBuilder.Entity<Brand>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Color>().ToTable("Colors").HasKey(e => e.Id);
            modelBuilder.Entity<Color>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Material>().ToTable("Materials").HasKey(e => e.Id);
            modelBuilder.Entity<Material>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Size>().ToTable("Sizes").HasKey(e => e.Id);
            modelBuilder.Entity<Size>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Attribute>().ToTable("Attributes").HasKey(e => e.Id);
            modelBuilder.Entity<Attribute>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Order>().ToTable("Orders").HasKey(e => e.Id);
            modelBuilder.Entity<Order>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Order>().HasMany(o => o.OrderItems).WithOne(oi => oi.Order).HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>().ToTable("OrderItems").HasKey(e => e.Id);
            modelBuilder.Entity<OrderItem>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<OrderItem>().HasOne(oi => oi.ProductSize).WithMany().HasForeignKey(oi => oi.TrackingId).HasPrincipalKey(ps => ps.TrackingId);

            base.OnModelCreating(modelBuilder);
        }
    }
}