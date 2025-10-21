namespace Shared.Messaging;

/// <summary>
/// 集成事件基类
/// </summary>
public abstract class IntegrationEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}

/// <summary>
/// 订单已创建事件
/// </summary>
public class OrderCreatedEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// 购物车清空事件
/// </summary>
public class BasketClearedEvent : IntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public int OrderId { get; set; }
}
