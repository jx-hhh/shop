using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Shared.Configuration;
using Shared.Messaging;
using StackExchange.Redis;
using System.Text;

namespace Shared.Extensions;

/// <summary>
/// 服务注册扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 JWT 认证
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                        ?? throw new InvalidOperationException("JwtSettings not configured");

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    /// <summary>
    /// 添加 Swagger 文档
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        string serviceName,
        string version = "v1")
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = serviceName,
                Version = version,
                Description = $"{serviceName} API Documentation"
            });

            // 添加 JWT Bearer 认证
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    /// 添加 OpenTelemetry 追踪
    /// </summary>
    public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        string serviceName,
        IConfiguration configuration)
    {
        var jaegerSettings = configuration.GetSection("Jaeger").Get<JaegerSettings>()
                           ?? new JaegerSettings();

        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri($"http://{jaegerSettings.AgentHost}:4317");
                    });
            });

        return services;
    }

    /// <summary>
    /// 添加 RabbitMQ 事件总线
    /// </summary>
    public static IServiceCollection AddRabbitMQEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMQSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>()
                             ?? new RabbitMQSettings();

        services.AddSingleton<IEventBus>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RabbitMQEventBus>>();
            return new RabbitMQEventBus(logger, sp, rabbitMQSettings.Host);
        });

        return services;
    }

    /// <summary>
    /// 添加 Redis 缓存
    /// </summary>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisSettings = configuration.GetSection("Redis").Get<RedisSettings>()
                          ?? new RedisSettings();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            return ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisSettings.ConnectionString;
        });

        return services;
    }

    /// <summary>
    /// 添加 Serilog 日志
    /// </summary>
    public static void AddSerilogLogging(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    /// <summary>
    /// 添加健康检查
    /// </summary>
    public static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // 可以根据配置添加更多健康检查
        // healthChecksBuilder.AddSqlite(connectionString);
        // healthChecksBuilder.AddRabbitMQ(rabbitConnectionString);

        return services;
    }

    /// <summary>
    /// 添加内存缓存（用于限流）
    /// </summary>
    public static IServiceCollection AddMemoryCacheForRateLimiting(this IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }

    /// <summary>
    /// 添加 CORS 策略
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, string policyName = "AllowAll")
    {
        services.AddCors(options =>
        {
            options.AddPolicy(policyName, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        return services;
    }
}
