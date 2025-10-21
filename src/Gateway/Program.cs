using Serilog;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ==================== 服务注册 ====================

// 1. 配置 Serilog 日志
builder.Services.AddSerilogLogging();
builder.Host.UseSerilog();

// 2. 添加 YARP 反向代理
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 3. 添加 JWT 认证（用于验证令牌）
builder.Services.AddJwtAuthentication(builder.Configuration);

// 4. 添加授权
builder.Services.AddAuthorization();

// 5. 添加 OpenTelemetry 追踪
builder.Services.AddOpenTelemetryTracing("Gateway", builder.Configuration);

// 6. 添加健康检查
builder.Services.AddHealthChecks();

// 7. 添加内存缓存（用于限流）
builder.Services.AddMemoryCacheForRateLimiting();

// 8. 添加 CORS
builder.Services.AddCorsPolicy();

var app = builder.Build();

// ==================== 中间件管道配置 ====================

// 1. 使用 CORS
app.UseCors("AllowAll");

// 2. 使用自定义中间件（相关性ID、日志、异常处理、限流）
app.UseCustomMiddlewares(includeRateLimiting: true);

// 3. 使用认证和授权
app.UseAuthentication();
app.UseAuthorization();

// 4. 映射反向代理
app.MapReverseProxy();

// 5. 映射健康检查端点
app.MapHealthChecks("/healthz");

Log.Information("API Gateway starting on {Urls}", builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000");

app.Run();
