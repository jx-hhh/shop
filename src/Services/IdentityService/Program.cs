using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ==================== 服务注册 ====================

// 1. 配置 Serilog 日志
builder.Services.AddSerilogLogging();
builder.Host.UseSerilog();

// 2. 添加控制器
builder.Services.AddControllers();

// 3. 配置数据库 (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. 配置 ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // 密码配置
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // 用户配置
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 5. 添加 JWT 认证
builder.Services.AddJwtAuthentication(builder.Configuration);

// 6. 添加授权
builder.Services.AddAuthorization();

// 7. 注册应用服务
builder.Services.AddScoped<IAuthService, AuthService>();

// 8. 添加 RabbitMQ 事件总线
builder.Services.AddRabbitMQEventBus(builder.Configuration);

// 9. 添加 OpenTelemetry 追踪
builder.Services.AddOpenTelemetryTracing("IdentityService", builder.Configuration);

// 10. 添加 Swagger 文档
builder.Services.AddSwaggerDocumentation("Identity Service");

// 11. 添加健康检查
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// 12. 添加内存缓存（用于限流）
builder.Services.AddMemoryCacheForRateLimiting();

// 13. 添加 CORS
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
    app.UseSwaggerDocumentation("Identity Service");
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
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
        Log.Information("Database created/verified successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while creating the database");
    }
}

Log.Information("Identity Service starting on {Urls}", builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5001");

app.Run();
