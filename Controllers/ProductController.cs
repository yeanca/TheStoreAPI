using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheStoreAPI.Infrastructure.Data;
using TheStoreAPI.Infrastructure.DTOs;

namespace TheStoreAPI.Controllers
{
    /// <summary>
    /// This controller handles all product catalog operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly TheStoreDbContext _context;
        private readonly ILogger<ProductController> _logger;

        private const int DefaultPageSize = 30;
        private const int MaxPageSize = 100;

        public ProductController(TheStoreDbContext context, ILogger<ProductController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a single product by its ID.
        /// </summary>
        /// <param name="id">The unique ID of the product.</param>
        /// <returns>A detailed view of the product, or a 404 if not found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            _logger.LogInformation("GetProduct called for Id: {Id}", id);

            if (id <= 0)
            {
                _logger.LogWarning("Invalid product ID provided: {Id}", id);
                return BadRequest("Invalid product ID.");
            }

            try
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.Id == id && p.IsActive && !p.IsDeleted)
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Color)
                    .Include(p => p.Material)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size)
                    .Include(p => p.ProductAttributes).ThenInclude(pa => pa.Attribute)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    _logger.LogWarning("Product with Id {Id} not found or is inactive.", id);
                    return NotFound($"Product with ID '{id}' not found or is inactive.");
                }

                var productDto = new ProductDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    MainImageUrl = product.MainImageUrl,
                    TotalStockQuantity = product.TotalStockQuantity,
                    IsActive = product.IsActive,
                    CategoryName = product.Category?.Name,
                    BrandName = product.Brand?.Name,
                    ColorName = product.Color?.Name,
                    MaterialName = product.Material?.Name,
                    Sizes = product.ProductSizes?.Select(ps => new ProductSizeDto
                    {
                        TrackingId = ps.TrackingId,
                        SizeName = ps.Size.Name,
                        StockQuantity = ps.StockQuantity
                    }).ToList(),
                    OtherImages = product.ProductImages?.Select(pi => new ProductImageDto
                    {
                        Id = pi.Id,
                        ImageUrl = pi.ImageUrl
                    }).ToList(),
                    Attributes = product.ProductAttributes?.Select(pa => new ProductAttributeDto
                    {
                        AttributeName = pa.Attribute.Name,
                        Value = pa.Value
                    }).ToList()
                };

                _logger.LogInformation("Product {Id} found successfully.", id);
                return Ok(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving product {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred processing your request.");
            }
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="productDto">The product creation DTO.</param>
        /// <returns>A confirmation of the created product.</returns>
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> CreateProduct([FromBody] ProductCreateDto productDto)
        {
            _logger.LogInformation("CreateProduct called for product '{ProductName}'.", productDto.Name);

        
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for product creation.");
                return BadRequest(ModelState);
            }

            
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var newProduct = new Product
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    CategoryId = productDto.CategoryId,
                    ColorId = productDto.ColorId,
                    MaterialId = productDto.MaterialId,
                    BrandId = productDto.BrandId,
                    MainImageUrl = productDto.MainImageUrl,
                    IsActive = true,
                    IsHot = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _context.Products.AddAsync(newProduct);
                await _context.SaveChangesAsync();

                
                if (productDto.OtherImageUrls != null)
                {
                    foreach (var imageUrl in productDto.OtherImageUrls)
                    {
                        await _context.ProductImages.AddAsync(new ProductImage { ProductId = newProduct.Id, ImageUrl = imageUrl });
                    }
                }

                
                var totalStock = 0;
                if (productDto.Sizes != null)
                {
                    foreach (var sizeDto in productDto.Sizes)
                    {
                        var trackingId = $"{productDto.Name.Substring(0, 2)}-{sizeDto.SizeId}";
                        await _context.ProductSizes.AddAsync(new ProductSize
                        {
                            TrackingId = trackingId,
                            ProductId = newProduct.Id,
                            SizeId = sizeDto.SizeId,
                            StockQuantity = sizeDto.StockQuantity,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                        totalStock += sizeDto.StockQuantity;
                    }
                }

                
                if (productDto.Attributes != null)
                {
                    foreach (var attrDto in productDto.Attributes)
                    {
                        await _context.ProductAttributes.AddAsync(new ProductAttribute
                        {
                            ProductId = newProduct.Id,
                            AttributeId = attrDto.AttributeId,
                            Value = attrDto.Value
                        });
                    }
                }

                newProduct.TotalStockQuantity = totalStock;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Product {ProductId} created successfully.", newProduct.Id);
                return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, newProduct);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while creating a product.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred processing your request.");
            }
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">The unique ID of the product to update.</param>
        /// <param name="productDto">The DTO containing the updated details.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto productDto)
        {
            _logger.LogInformation("UpdateProduct called for Id: {Id}", id);

            if (id != productDto.Id)
            {
                return BadRequest("Product ID in URL does not match ID in the body.");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with Id {Id} not found for update.", id);
                return NotFound();
            }

            try
            {
                if (productDto.Name != null) product.Name = productDto.Name;
                if (productDto.Description != null) product.Description = productDto.Description;
                if (productDto.Price.HasValue) product.Price = productDto.Price.Value;
                if (productDto.CategoryId.HasValue) product.CategoryId = productDto.CategoryId.Value;
                if (productDto.ColorId.HasValue) product.ColorId = productDto.ColorId.Value;
                if (productDto.MaterialId.HasValue) product.MaterialId = productDto.MaterialId.Value;
                if (productDto.BrandId.HasValue) product.BrandId = productDto.BrandId.Value;
                if (productDto.MainImageUrl != null) product.MainImageUrl = productDto.MainImageUrl;
                if (productDto.IsActive.HasValue) product.IsActive = productDto.IsActive.Value;
                if (productDto.IsHot.HasValue) product.IsHot = productDto.IsHot.Value;

                product.UpdatedAt = DateTime.Now;

                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} updated successfully.", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency error occurred while updating product {ProductId}.", id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating product {ProductId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred processing your request.");
            }
        }
        /// <summary>
        /// Soft deletes a product by its ID.
        /// </summary>
        /// <param name="id">The unique ID of the product.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("DeleteProduct called for Id: {Id}", id);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with Id {Id} not found for deletion.", id);
                return NotFound();
            }

            try
            {
                // Soft delete the product
                product.IsDeleted = true;
                product.UpdatedAt = DateTime.Now;

                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} soft deleted successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting product {ProductId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred processing your request.");
            }
        }

        /// <summary>
        /// Retrieves a list of products based on various filters.
        /// </summary>
        /// <param name="urlId">A hyphen-separated string of IDs for filtering.</param>
        /// <param name="sortId">An integer to sort the results.</param>
        /// <param name="pageIndex">The page number to retrieve.</param>
        /// <param name="pageSize">The number of products per page.</param>
        /// <returns>A paginated list of products.</returns>
        [HttpGet("products/{urlId?}")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts(    string? urlId = null,    int? sortId = null,    int pageIndex = 0,    int pageSize = DefaultPageSize)
        {
            _logger.LogInformation("GetProducts called with urlId: {UrlId}, SortId: {SortId}, PageIndex: {PageIndex}, PageSize: {PageSize}",
                urlId, sortId, pageIndex, pageSize);

            if (pageIndex < 0) pageIndex = 0;
            if (pageSize <= 0 || pageSize > MaxPageSize) pageSize = DefaultPageSize;

            
            int? categoryId = null;
            int? colorId = null;
            int? materialId = null;
            int? brandId = null;

            
            if (!string.IsNullOrEmpty(urlId)) // the url comes in this format: 10018-10023-93894-0-men-shoes?sortId=3
            {
                var parts = urlId.Split('-');

                
                if (parts.Length > 0 && int.TryParse(parts[0], out int parsedCategoryId))
                {
                    categoryId = parsedCategoryId;
                }

              
                if (parts.Length > 1 && int.TryParse(parts[1], out int parsedColorId))
                {
                    colorId = parsedColorId;
                }

                if (parts.Length > 2 && int.TryParse(parts[2], out int parsedMaterialId))
                {
                    materialId = parsedMaterialId;
                }
                if (parts.Length > 3 && int.TryParse(parts[3], out int parsedBrandId))
                {
                    brandId = parsedBrandId;
                }
            }

            try
            {
                
                IQueryable<Product> query = _context.Products
                    .AsNoTracking()
                    .Where(p => p.IsActive && !p.IsDeleted && p.TotalStockQuantity > 0);

             
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }
                if (colorId.HasValue && colorId.Value > 0)
                {
                    query = query.Where(p => p.ColorId == colorId.Value);
                }
                if (materialId.HasValue && materialId.Value > 0)
                {
                    query = query.Where(p => p.MaterialId == materialId.Value);
                }
                if (brandId.HasValue && brandId.Value > 0)
                {
                    query = query.Where(p => p.BrandId == brandId.Value);
                }


                switch (sortId)
                {
                    case 1: // Newest first
                        query = query.OrderByDescending(p => p.CreatedAt);
                        break;
                    case 2: // Price High to Low
                        query = query.OrderByDescending(p => p.Price);
                        break;
                    case 3: // Price Low to High
                        query = query.OrderBy(p => p.Price);
                        break;
                    case 4: // Title A-Z
                        query = query.OrderBy(p => p.Name);
                        break;
                    case 5: // Title Z-A
                        query = query.OrderByDescending(p => p.Name);
                        break;
                    default: // Default sort by Name
                        query = query.OrderBy(p => p.Name);
                        break;
                }

               
                var pagedQuery = query.Skip(pageIndex * pageSize).Take(pageSize);

               
                var productDtos = await pagedQuery
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        MainImageUrl = p.MainImageUrl,
                        TotalStockQuantity = p.TotalStockQuantity,
                        IsActive = p.IsActive,
                        CategoryName = p.Category.Name,
                        BrandName = p.Brand.Name,
                        ColorName = p.Color.Name,
                        MaterialName = p.Material.Name,
                        Sizes = p.ProductSizes.Select(ps => new ProductSizeDto
                        {
                            TrackingId = ps.TrackingId,
                            SizeName = ps.Size.Name,
                            StockQuantity = ps.StockQuantity
                        }).ToList(),
                        OtherImages = p.ProductImages.Select(pi => new ProductImageDto
                        {
                            Id = pi.Id,
                            ImageUrl = pi.ImageUrl
                        }).ToList(),
                        Attributes = p.ProductAttributes.Select(pa => new ProductAttributeDto
                        {
                            AttributeName = pa.Attribute.Name,
                            Value = pa.Value
                        }).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} products. Filters applied: CategoryId: {CategoryId}, ColorId: {ColorId}, MaterialId: {MaterialId}, BrandId: {BrandId}, SortId: {SortId}",
                    productDtos.Count, categoryId, colorId, materialId, brandId, sortId);
                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving products with the given parameters.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred processing your request.");
            }
        }

        /// <summary>
        /// Retrieves a paginated list of featured products.
        /// </summary>
        /// <param name="pageIndex">The page number to retrieve.</param>
        /// <param name="pageSize">The number of products per page.</param>
        /// <returns>A paginated list of featured products.</returns>
        [HttpGet("featured")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetFeaturedProducts(int pageIndex = 0, int pageSize = DefaultPageSize)
        {
            _logger.LogInformation("GetFeaturedProducts called with PageIndex: {PageIndex}, PageSize: {PageSize}", pageIndex, pageSize);

            if (pageIndex < 0) pageIndex = 0;
            if (pageSize <= 0 || pageSize > MaxPageSize) pageSize = DefaultPageSize;

            try
            {
                var query = _context.Products
                    .AsNoTracking()
                    .Where(p => p.IsHot && p.IsActive && !p.IsDeleted && p.TotalStockQuantity > 0)
                    .OrderBy(p => p.Name)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Color)
                    .Include(p => p.Material)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size)
                    .Include(p => p.ProductAttributes).ThenInclude(pa => pa.Attribute);

                var products = await query.ToListAsync();

                var productDtos = products.Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    MainImageUrl = p.MainImageUrl,
                    TotalStockQuantity = p.TotalStockQuantity,
                    IsActive = p.IsActive,
                    CategoryName = p.Category?.Name,
                    BrandName = p.Brand?.Name,
                    ColorName = p.Color?.Name,
                    MaterialName = p.Material?.Name,
                    Sizes = p.ProductSizes?.Select(ps => new ProductSizeDto
                    {
                        TrackingId = ps.TrackingId,
                        SizeName = ps.Size.Name,
                        StockQuantity = ps.StockQuantity
                    }).ToList(),
                    OtherImages = p.ProductImages?.Select(pi => new ProductImageDto
                    {
                        Id = pi.Id,
                        ImageUrl = pi.ImageUrl
                    }).ToList(),
                    Attributes = p.ProductAttributes?.Select(pa => new ProductAttributeDto
                    {
                        AttributeName = pa.Attribute.Name,
                        Value = pa.Value
                    }).ToList()
                }).ToList();

                _logger.LogInformation("Retrieved {Count} featured products.", productDtos.Count);
                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving featured products.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred processing your request.");
            }
        }



        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}