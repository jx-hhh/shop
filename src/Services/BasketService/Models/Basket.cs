using Shared.Entities;

namespace BasketService.Models;

/// <summary>
/// 购物车实体
/// </summary>
public class Basket : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public List<BasketItem> Items { get; set; } = new();
}

/// <summary>
/// 购物车商品项
/// </summary>
public class BasketItem : BaseEntity
{
    public int BasketId { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public string? BookImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
