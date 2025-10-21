namespace Shared.DTOs;

/// <summary>
/// 购物车 DTO
/// </summary>
public class BasketDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
    public long CreatedAt { get; set; } // 秒级时间戳
    public long? UpdatedAt { get; set; } // 秒级时间戳
}

/// <summary>
/// 购物车商品项 DTO
/// </summary>
public class BasketItemDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public string? BookImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// 添加到购物车请求
/// </summary>
public class AddToBasketRequest
{
    public int BookId { get; set; }
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// 更新购物车商品数量请求
/// </summary>
public class UpdateBasketItemRequest
{
    public int BasketItemId { get; set; }
    public int Quantity { get; set; }
}
