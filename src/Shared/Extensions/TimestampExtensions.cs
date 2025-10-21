namespace Shared.Extensions;

/// <summary>
/// 时间戳转换扩展方法
/// </summary>
public static class TimestampExtensions
{
    /// <summary>
    /// 将秒级时间戳转换为 DateTime
    /// </summary>
    public static DateTime ToDateTime(this long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
    }

    /// <summary>
    /// 将秒级时间戳转换为本地 DateTime
    /// </summary>
    public static DateTime ToLocalDateTime(this long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
    }

    /// <summary>
    /// 将秒级时间戳转换为格式化字符串 (UTC)
    /// </summary>
    public static string ToDateTimeString(this long timestamp, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return timestamp.ToDateTime().ToString(format);
    }

    /// <summary>
    /// 将秒级时间戳转换为本地格式化字符串
    /// </summary>
    public static string ToLocalDateTimeString(this long timestamp, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return timestamp.ToLocalDateTime().ToString(format);
    }

    /// <summary>
    /// 将 DateTime 转换为秒级时间戳
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    }

    /// <summary>
    /// 获取当前秒级时间戳
    /// </summary>
    public static long GetCurrentTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// 可空时间戳转换为字符串
    /// </summary>
    public static string? ToDateTimeString(this long? timestamp, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return timestamp?.ToDateTimeString(format);
    }
}
