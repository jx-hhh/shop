namespace Shared.Entities;

/// <summary>
/// 基础实体类，所有实体都继承此类
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    /// <summary>
    /// 创建时间（秒级时间戳）
    /// </summary>
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    /// 更新时间（秒级时间戳，可为空）
    /// </summary>
    public long? UpdatedAt { get; set; }
}
