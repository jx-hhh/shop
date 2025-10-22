using BasketService.Data;
using BasketService.EventHandlers;
using Serilog;
using Shared.Extensions;
using Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

// ==================== 服务注册 ====================

// 1. 配置 Serilog 日志
builder.Services.AddSerilogLogging();
builder.Host.UseSerilog();

// 2. 添加控制器
builder.Services.AddControllers();

// 3. 注册数据访问层
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddSingleton<DatabaseInitializer>();

// 4. 注册业务服务
builder.Services.AddScoped<BasketService.Services.IBasketService, BasketService.Services.BasketService>();

// 5. 注册 CatalogService HTTP 客户端
var catalogServiceUrl = builder.Configuration["ServiceUrls:CatalogService"] ?? "http://localhost:5002";
builder.Services.AddHttpClient<BasketService.Services.ICatalogServiceClient, BasketService.Services.CatalogServiceClient>(client =>
{
    client.BaseAddress = new Uri(catalogServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// 6. 注册事件处理器
builder.Services.AddScoped<OrderCreatedEventHandler>();
builder.Services.AddScoped<IIntegrationEventHandler<OrderCreatedEvent>>(sp =>
    sp.GetRequiredService<OrderCreatedEventHandler>());

// 7. 添加 JWT 认证
builder.Services.AddJwtAuthentication(builder.Configuration);

// 8. 添加授权
builder.Services.AddAuthorization();

// 9. 添加 RabbitMQ 事件总线
builder.Services.AddRabbitMQEventBus(builder.Configuration);

// 10. 添加 OpenTelemetry 追踪
builder.Services.AddOpenTelemetryTracing("BasketService", builder.Configuration);

// 11. 添加 Swagger 文档
builder.Services.AddSwaggerDocumentation("Basket Service");

// 12. 添加健康检查
builder.Services.AddHealthChecks();

// 13. 添加内存缓存（用于限流）
builder.Services.AddMemoryCacheForRateLimiting();

// 14. 添加 CORS
builder.Services.AddCorsPolicy();

var app = builder.Build();

// ==================== 中间件管道配置 ====================

// 1. 使用 CORS
app.UseCors("AllowAll");

// 2. 使用自定义中间件（相关性ID、日志、异常处理、限流）
app.UseCustomMiddlewares(includeRateLimiting: true);

// 3. 使用 Swagger（开发环境）
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation("Basket Service");
}

// 4. 使用 HTTPS 重定向
app.UseHttpsRedirection();

// 5. 使用认证和授权
app.UseAuthentication();
app.UseAuthorization();

// 6. 映射控制器
app.MapControllers();

// 7. 映射健康检查端点
app.MapHealthChecks("/healthz");

// ==================== 数据库初始化 ====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbInitializer = services.GetRequiredService<DatabaseInitializer>();
        await dbInitializer.InitializeAsync();
        Log.Information("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while initializing the database");
    }
}

// ==================== 订阅事件 ====================
var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<OrderCreatedEvent, OrderCreatedEventHandler>();
Log.Information("Subscribed to OrderCreatedEvent");

Log.Information("Basket Service starting on {Urls}", builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5003");

app.Run();
