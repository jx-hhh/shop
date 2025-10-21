using Microsoft.AspNetCore.Builder;
using Shared.Middleware;

namespace Shared.Extensions;

/// <summary>
/// 应用程序构建器扩展方法（用于配置中间件管道）
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 使用所有自定义中间件
    /// </summary>
    public static IApplicationBuilder UseCustomMiddlewares(
        this IApplicationBuilder app,
        bool includeRateLimiting = true)
    {
        // 中间件执行顺序很重要！

        // 1. 相关性 ID（最先执行，为后续中间件提供追踪 ID）
        app.UseMiddleware<CorrelationIdMiddleware>();

        // 2. 请求日志
        app.UseMiddleware<RequestLoggingMiddleware>();

        // 3. 异常处理（捕获所有后续中间件的异常）
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // 4. 限流（可选）
        if (includeRateLimiting)
        {
            app.UseMiddleware<RateLimitingMiddleware>();
        }

        return app;
    }

    /// <summary>
    /// 使用 Swagger UI
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(
        this IApplicationBuilder app,
        string serviceName,
        string version = "v1")
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{serviceName} {version}");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
