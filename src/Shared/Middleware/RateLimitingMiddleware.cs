using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Shared.DTOs;
using System.Net;
using System.Text.Json;

namespace Shared.Middleware;

/// <summary>
/// 限流中间件
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<RateLimitingMiddleware> logger,
        int maxRequests = 100,
        int timeWindowSeconds = 60)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _maxRequests = maxRequests;
        _timeWindow = TimeSpan.FromSeconds(timeWindowSeconds);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var cacheKey = $"RateLimit_{clientId}";

        var requestCount = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _timeWindow;
            return 0;
        });

        requestCount++;

        if (requestCount > _maxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
            await HandleRateLimitExceeded(context);
            return;
        }

        _cache.Set(cacheKey, requestCount, _timeWindow);
        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // 优先使用用户 ID，其次使用 IP 地址
        var userId = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"User_{userId}";
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"IP_{ipAddress}";
    }

    private static async Task HandleRateLimitExceeded(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.ErrorResponse(
            "Rate limit exceeded. Please try again later.",
            new List<string> { "Too many requests" }
        );

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}
