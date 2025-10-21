using Shared.Entities;

namespace OrderingService.Models;

/// <summary>
/// 订单实体
/// </summary>
public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled
    public string ShippingAddress { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
}

/// <summary>
/// 订单商品项
/// </summary>
public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
