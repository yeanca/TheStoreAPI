using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TheStoreAPI.Infrastructure.Data;
using TheStoreAPI.Infrastructure.DTOs;
using TheStoreAPI.Infrastructure.Enums;

namespace TheStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly TheStoreDbContext _context;
        private readonly ILogger<OrderController> _logger;
        private readonly IConfiguration _configuration;

        public OrderController(TheStoreDbContext context, ILogger<OrderController> logger, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        [HttpGet("active")]
        public async Task<ActionResult<OrderDto>> GetActiveOrder()
        {
            var anonymousUserId = User.FindFirst("AnonymousUserId")?.Value;
            if (string.IsNullOrEmpty(anonymousUserId))
            {
                return Unauthorized("A valid user identifier could not be found.");
            }

            try
            {
                var activeOrder = await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductSize)
                            .ThenInclude(ps => ps.Product)
                    .FirstOrDefaultAsync(o => o.AnonymousUserId == anonymousUserId && o.Status == (int)OrderStatus.Pending);

                if (activeOrder == null)
                {
                    _logger.LogInformation("No active order found for user {AnonymousUserId}.", anonymousUserId);
                    return NotFound("No active order found.");
                }

     
                var orderDto = new OrderDto
                {
                    OrderId = activeOrder.Id,
                    OrderDate = activeOrder.OrderDate,
                    TotalPrice = activeOrder.TotalPrice,
                    Status = activeOrder.Status,
                    Items = activeOrder.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        TrackingId = oi.TrackingId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        ProductName = oi.ProductSize.Product.Name
                    }).ToList()
                };

                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the active order for user {AnonymousUserId}.", anonymousUserId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        
        [HttpPost("add-to-cart")]
        public async Task<ActionResult<OrderConfirmationDto>> AddToCart([FromBody] OrderItemCreateDto itemDto)
        {
            var anonymousUserId = User.FindFirst("AnonymousUserId")?.Value;
            if (string.IsNullOrEmpty(anonymousUserId))
            {
                return Unauthorized("A valid user identifier could not be found.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var productSize = await _context.ProductSizes
                    .Include(ps => ps.Product)
                    .FirstOrDefaultAsync(ps => ps.TrackingId == itemDto.TrackingId);

                if (productSize == null || productSize.StockQuantity < itemDto.Quantity)
                {
                    return BadRequest("Insufficient stock or product not found.");
                }

                var activeOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.AnonymousUserId == anonymousUserId && o.Status == (int)OrderStatus.Pending);

                if (activeOrder == null)
                {
                    // Create a new order if no active one exists
                    activeOrder = new Order
                    {
                        OrderDate = DateTime.Now,
                        AnonymousUserId = anonymousUserId,
                        Status = (int)OrderStatus.Pending,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        OrderItems = new List<OrderItem>()
                    };
                    _context.Orders.Add(activeOrder);
                    await _context.SaveChangesAsync();
                }

                var existingItem = activeOrder.OrderItems
                    .FirstOrDefault(oi => oi.TrackingId == itemDto.TrackingId);

                if (existingItem != null)
                {
                    // Update qty if item already in cart
                    existingItem.Quantity += itemDto.Quantity;
                }
                else
                {
                    // Add new item to cart
                    activeOrder.OrderItems.Add(new OrderItem
                    {
                        TrackingId = itemDto.TrackingId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = productSize.Product.Price,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

               
                productSize.StockQuantity -= itemDto.Quantity;
                activeOrder.TotalPrice = activeOrder.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new OrderConfirmationDto
                {
                    OrderId = activeOrder.Id,
                    TotalPrice = activeOrder.TotalPrice, 
                    Status = activeOrder.Status,
                    Message = "Item added to cart successfully."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while adding to cart for user {AnonymousUserId}.", anonymousUserId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("{orderId}")]
        [AllowAnonymous] 
        public async Task<ActionResult<OrderDto>> GetOrderById(long orderId)
        {
            _logger.LogInformation("GetOrderById called for OrderId: {OrderId}", orderId);

            if (orderId <= 0)
            {
                _logger.LogWarning("Invalid order ID provided: {OrderId}", orderId);
                return BadRequest("Invalid order ID.");
            }

            try
            {
                var order = await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductSize)
                            .ThenInclude(ps => ps.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order with Id {OrderId} not found.", orderId);
                    return NotFound($"Order with ID '{orderId}' not found.");
                }

                var orderDto = new OrderDto
                {
                    OrderId = order.Id,
                    OrderDate = order.OrderDate,
                    TotalPrice = order.TotalPrice,
                    Status = order.Status,
                    Items = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        TrackingId = oi.TrackingId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        ProductName = oi.ProductSize.Product.Name
                    }).ToList()
                };

                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving order {OrderId}.", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }


        [HttpDelete("remove-item/{orderItemId}")]
        public async Task<IActionResult> RemoveOrderItem(int orderItemId)
        {
            var anonymousUserId = User.FindFirst("AnonymousUserId")?.Value;
            if (string.IsNullOrEmpty(anonymousUserId))
            {
                return Unauthorized("A valid user identifier could not be found.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var orderItem = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.ProductSize)
                    .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

                if (orderItem == null || orderItem.Order.AnonymousUserId != anonymousUserId || orderItem.Order.Status != (int)OrderStatus.Pending)
                {
                    _logger.LogWarning("Attempted to remove non-existent or invalid order item {OrderItemId}.", orderItemId);
                    return NotFound("The order item was not found or cannot be removed.");
                }

                
                orderItem.ProductSize.StockQuantity += orderItem.Quantity;

               
                _context.OrderItems.Remove(orderItem);


                await _context.SaveChangesAsync();

                
                var isOrderEmpty = !await _context.OrderItems.AnyAsync(oi => oi.OrderId == orderItem.OrderId);
                if (isOrderEmpty)
                {
                    _context.Orders.Remove(orderItem.Order);
                }
                var activeOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderItem.OrderId);

                if (activeOrder != null)
                {
                    activeOrder.TotalPrice = activeOrder.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);

                    if (!activeOrder.OrderItems.Any())
                    {
                        _context.Orders.Remove(activeOrder);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Order item {OrderItemId} removed successfully.", orderItemId);
                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while removing order item {OrderItemId}.", orderItemId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        
        [HttpPost("checkout")]
        public async Task<ActionResult<OrderConfirmationDto>> Checkout()
        {
            var anonymousUserId = User.FindFirst("AnonymousUserId")?.Value;
            if (string.IsNullOrEmpty(anonymousUserId))
            {
                return Unauthorized("A valid user identifier could not be found.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var activeOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                    .FirstOrDefaultAsync(o => o.AnonymousUserId == anonymousUserId && o.Status == (int)OrderStatus.Pending);

                if (activeOrder == null)
                {
                    _logger.LogWarning("Checkout failed: No active order found for user {AnonymousUserId}.", anonymousUserId);
                    return NotFound("No active order to checkout.");
                }

                
                var totalOrderPrice = 0.0m;
                foreach (var item in activeOrder.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductSize.ProductId);
                    if (item.Quantity > item.ProductSize.StockQuantity)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("Checkout failed: Insufficient stock for {ProductName}.", product.Name);
                        return BadRequest($"Insufficient stock for {product.Name}. Please review your cart.");
                    }
                    totalOrderPrice += product.Price * item.Quantity;
                }

                
                activeOrder.TotalPrice = totalOrderPrice;
                activeOrder.Status = (int)OrderStatus.Packed; 
                activeOrder.UpdatedAt = DateTime.Now;

                _context.Orders.Update(activeOrder);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Checkout successful for Order {OrderId} for user {AnonymousUserId}.", activeOrder.Id, anonymousUserId);

                return Ok(new OrderConfirmationDto
                {
                    OrderId = activeOrder.Id,
                    TotalPrice = activeOrder.TotalPrice,
                    Status = activeOrder.Status,
                    Message = "Your order has been placed successfully."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred during checkout for user {AnonymousUserId}.", anonymousUserId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during checkout.");
            }
        }
    }
}