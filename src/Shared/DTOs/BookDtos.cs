namespace Shared.DTOs;

/// <summary>
/// 图书商品 DTO
/// </summary>
public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public long CreatedAt { get; set; } // 秒级时间戳
    public long? UpdatedAt { get; set; } // 秒级时间戳
}

/// <summary>
/// 创建图书请求
/// </summary>
public class CreateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

/// <summary>
/// 更新图书请求
/// </summary>
public class UpdateBookRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

/// <summary>
/// 图书搜索请求
/// </summary>
public class SearchBooksRequest : PagedRequest
{
    public string? Keyword { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
