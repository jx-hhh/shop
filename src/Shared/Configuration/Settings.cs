namespace Shared.Configuration;

/// <summary>
/// JWT 配置
/// </summary>
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

/// <summary>
/// RabbitMQ 配置
/// </summary>
public class RabbitMQSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}

/// <summary>
/// Redis 配置
/// </summary>
public class RedisSettings
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public int CacheExpirationMinutes { get; set; } = 30;
}

/// <summary>
/// Jaeger 配置
/// </summary>
public class JaegerSettings
{
    public string AgentHost { get; set; } = "localhost";
    public int AgentPort { get; set; } = 6831;
}
