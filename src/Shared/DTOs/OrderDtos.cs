namespace Shared.DTOs;

/// <summary>
/// 订单 DTO
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Processing, Shipped, Delivered, Cancelled
    public string ShippingAddress { get; set; } = string.Empty;
    public long CreatedAt { get; set; } // 秒级时间戳
    public long? UpdatedAt { get; set; } // 秒级时间戳
}

/// <summary>
/// 订单商品项 DTO
/// </summary>
public class OrderItemDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Price * Quantity;
}

/// <summary>
/// 创建订单请求
/// </summary>
public class CreateOrderRequest
{
    public string ShippingAddress { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
}

/// <summary>
/// 更新订单状态请求
/// </summary>
public class UpdateOrderStatusRequest
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// 订单查询请求
/// </summary>
public class OrderQueryRequest : PagedRequest
{
    public string? Status { get; set; }
    public long? StartTime { get; set; } // 秒级时间戳
    public long? EndTime { get; set; } // 秒级时间戳
}
