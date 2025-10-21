# 🚀 在线书店微服务项目

一个基于 **ASP.NET 8**、**Vue 3** 和 **SQLite** 的完整微服务架构的在线书店系统。

## 📋 项目概述

这是一个生产级的微服务项目，展示了现代应用程序的完整架构，包括：

- ✅ **微服务架构**：4 个独立的后端服务
- ✅ **API 网关**：使用 YARP 统一入口
- ✅ **消息队列**：RabbitMQ 实现异步通信
- ✅ **分布式缓存**：Redis 提升性能
- ✅ **分布式追踪**：OpenTelemetry + Jaeger
- ✅ **容器化**：Docker + Docker Compose 一键部署
- ✅ **现代前端**：Vue 3 + Element Plus

## 🏗️ 技术栈

### 后端
- **框架**: ASP.NET 8 Web API
- **ORM**: Dapper（轻量级）
- **数据库**: SQLite（每个微服务独立）
- **认证**: ASP.NET Core Identity + JWT Bearer
- **消息队列**: RabbitMQ
- **缓存**: Redis（StackExchange.Redis）
- **日志**: Serilog（结构化日志）
- **追踪**: OpenTelemetry + Jaeger
- **API 网关**: YARP (Yet Another Reverse Proxy)
- **文档**: Swagger/OpenAPI

### 前端
- **框架**: Vue 3 + Composition API
- **UI 组件**: Element Plus
- **状态管理**: Pinia
- **路由**: Vue Router
- **HTTP 客户端**: Axios
- **构建工具**: Vite

### 基础设施
- **容器化**: Docker + Docker Compose
- **反向代理**: Nginx（生产环境）

## 📁 项目结构

```
OnlineBookstore/
├── src/
│   ├── Gateway/                    # API 网关 (YARP)
│   │   ├── Program.cs
│   │   ├── appsettings.json       # 路由配置
│   │   └── Dockerfile
│   │
│   ├── Services/
│   │   ├── IdentityService/       # 认证服务 (端口: 5001)
│   │   │   ├── Models/            # 用户实体、RefreshToken
│   │   │   ├── Data/              # DbContext
│   │   │   ├── Services/          # AuthService
│   │   │   └── Controllers/       # AuthController
│   │   │
│   │   ├── CatalogService/        # 商品目录服务 (端口: 5002)
│   │   │   ├── Models/            # Book 实体
│   │   │   ├── Data/              # BookRepository (Dapper)
│   │   │   ├── Services/          # BookService + CacheService
│   │   │   └── Controllers/       # BooksController
│   │   │
│   │   ├── BasketService/         # 购物车服务 (端口: 5003)
│   │   │   ├── Models/            # Basket、BasketItem
│   │   │   ├── Data/              # BasketRepository
│   │   │   ├── Services/          # BasketService
│   │   │   ├── EventHandlers/     # OrderCreatedEventHandler
│   │   │   └── Controllers/       # BasketController
│   │   │
│   │   └── OrderingService/       # 订单服务 (端口: 5004)
│   │       ├── Models/            # Order、OrderItem
│   │       ├── Data/              # OrderRepository
│   │       ├── Services/          # OrderService (发布事件)
│   │       └── Controllers/       # OrdersController
│   │
│   └── Shared/                     # 共享项目
│       ├── Entities/              # BaseEntity
│       ├── DTOs/                  # 所有 DTOs
│       ├── Middleware/            # 4 个独立中间件
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   ├── RequestLoggingMiddleware.cs
│       │   ├── CorrelationIdMiddleware.cs
│       │   └── RateLimitingMiddleware.cs
│       ├── Messaging/             # RabbitMQ 事件总线
│       ├── Configuration/         # 配置类
│       └── Extensions/            # 扩展方法
│           ├── TimestampExtensions.cs
│           ├── ServiceCollectionExtensions.cs
│           └── ApplicationBuilderExtensions.cs
│
├── frontend/                       # Vue 3 前端 (端口: 5173)
│   ├── src/
│   │   ├── api/                   # API 调用
│   │   ├── stores/                # Pinia 状态管理
│   │   ├── router/                # 路由配置
│   │   ├── views/                 # 页面组件
│   │   │   ├── Login.vue
│   │   │   ├── Register.vue
│   │   │   ├── Home.vue           # 商品列表
│   │   │   ├── BookDetail.vue
│   │   │   ├── Basket.vue         # 购物车
│   │   │   ├── Orders.vue         # 订单列表
│   │   │   └── OrderDetail.vue
│   │   ├── utils/                 # 工具函数
│   │   │   ├── timestamp.js       # 时间戳转换
│   │   │   └── request.js         # Axios 配置
│   │   ├── App.vue
│   │   └── main.js
│   ├── package.json
│   ├── vite.config.js
│   └── Dockerfile
│
├── docker-compose.yml              # Docker 编排文件
├── SETUP.md                        # 环境配置指南
├── PROJECT_STATUS.md               # 项目进度报告
└── README.md                       # 本文件
```

## 🎯 核心特性

### 1. 时间戳统一处理
所有时间字段使用**秒级时间戳**（long 类型），前后端统一：
- 后端：`long CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()`
- 前端：`formatTimestamp(timestamp)` 转换为可读字符串
- 工具类：`TimestampExtensions.cs` 和 `timestamp.js`

### 2. 中间件架构
每个中间件都是**独立的类文件**，通过扩展方法统一配置：
```csharp
// Program.cs 中简洁配置
app.UseCustomMiddlewares(includeRateLimiting: true);
```

执行顺序：
1. CorrelationIdMiddleware（跨服务请求追踪）
2. RequestLoggingMiddleware（请求日志）
3. ExceptionHandlingMiddleware（异常处理）
4. RateLimitingMiddleware（限流保护）

### 3. 微服务通信
- **同步通信**：HTTP REST API（通过 Gateway）
- **异步通信**：RabbitMQ 事件总线
  - 订单创建 → 购物车清空
  - 发布/订阅模式

### 4. 可观测性
- **日志**：Serilog 结构化日志
- **追踪**：OpenTelemetry + Jaeger UI
- **健康检查**：每个服务的 `/healthz` 端点
- **监控**：相关性 ID 跟踪请求链路

## 🚀 快速开始

### 前置要求

1. **.NET 8 SDK**
   ```bash
   dotnet --version  # 应显示 8.0.x
   ```

2. **Node.js 18+**
   ```bash
   node --version
   npm --version
   ```

3. **Docker & Docker Compose**
   ```bash
   docker --version
   docker-compose --version
   ```

### 方式1：使用 Docker Compose（推荐）

```bash
# 1. 启动所有服务
docker-compose up --build

# 2. 访问应用
# - 前端: http://localhost:5173
# - API 网关: http://localhost:5000
# - RabbitMQ 管理界面: http://localhost:15672 (guest/guest)
# - Jaeger UI: http://localhost:16686
```

### 方式2：本地开发

```bash
# 1. 启动基础设施（RabbitMQ、Redis、Jaeger）
docker-compose up rabbitmq redis jaeger -d

# 2. 启动后端服务（分别在不同终端）
cd src/Services/IdentityService && dotnet run
cd src/Services/CatalogService && dotnet run
cd src/Services/BasketService && dotnet run
cd src/Services/OrderingService && dotnet run
cd src/Gateway && dotnet run

# 3. 启动前端
cd frontend
npm install
npm run dev
```

## 📊 服务端口

| 服务 | 端口 | Swagger | 说明 |
|------|------|---------|------|
| Gateway | 5000 | - | API 网关统一入口 |
| IdentityService | 5001 | http://localhost:5001/swagger | 用户注册、登录、JWT |
| CatalogService | 5002 | http://localhost:5002/swagger | 图书CRUD、搜索、Redis缓存 |
| BasketService | 5003 | http://localhost:5003/swagger | 购物车管理、事件消费 |
| OrderingService | 5004 | http://localhost:5004/swagger | 订单创建、查询、事件发布 |
| Frontend | 5173 | - | Vue 3 用户界面 |
| RabbitMQ | 5672, 15672 | http://localhost:15672 | 消息队列 |
| Redis | 6379 | - | 分布式缓存 |
| Jaeger | 16686 | http://localhost:16686 | 分布式追踪 UI |

## 🔑 API 示例

### 1. 用户注册
```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}
```

### 2. 用户登录
```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Password123!"
}
```

### 3. 搜索图书
```http
GET http://localhost:5000/api/books/search?keyword=Clean&pageNumber=1&pageSize=10
```

### 4. 添加到购物车（需要认证）
```http
POST http://localhost:5000/api/basket/items
Authorization: Bearer {your_jwt_token}
Content-Type: application/json

{
  "bookId": 1,
  "quantity": 2
}
```

### 5. 创建订单（需要认证）
```http
POST http://localhost:5000/api/orders
Authorization: Bearer {your_jwt_token}
Content-Type: application/json

{
  "shippingAddress": "北京市朝阳区xxx街道xxx号",
  "contactPhone": "13800138000"
}
```

## 📸 功能展示

### 主要功能
1. ✅ 用户注册/登录
2. ✅ 图书浏览、搜索、分类筛选
3. ✅ 图书详情查看
4. ✅ 购物车管理（增删改查）
5. ✅ 订单创建与查询
6. ✅ 订单状态追踪

### 技术亮点
- 📦 微服务架构（4个独立服务）
- 🔐 JWT 认证授权
- 💾 每个服务独立 SQLite 数据库
- 📨 RabbitMQ 异步消息通信
- ⚡ Redis 缓存提升性能
- 📊 OpenTelemetry 分布式追踪
- 🔍 Serilog 结构化日志
- 🛡️ API 限流保护
- 📝 Swagger API 文档
- 🐳 Docker 容器化部署

## 🛠️ 开发指南

### 添加新的微服务

1. 创建项目结构
2. 添加 Shared 项目引用
3. 在 Program.cs 中配置：
```csharp
// 服务注册
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRabbitMQEventBus(builder.Configuration);
builder.Services.AddOpenTelemetryTracing("YourService", builder.Configuration);

// 中间件配置
app.UseCustomMiddlewares(includeRateLimiting: true);
```

4. 在 Gateway 的 appsettings.json 中添加路由
5. 在 docker-compose.yml 中添加服务配置

### 数据库迁移

SQLite 数据库会在首次运行时自动创建。如需重置：
```bash
# 删除数据库文件
rm src/Services/*/*.db

# 或在 Docker 中重新构建
docker-compose down -v
docker-compose up --build
```

## 📚 学习资源

- [ASP.NET Core 文档](https://docs.microsoft.com/aspnet/core)
- [Vue 3 文档](https://vuejs.org/)
- [Element Plus](https://element-plus.org/)
- [YARP 文档](https://microsoft.github.io/reverse-proxy/)
- [OpenTelemetry](https://opentelemetry.io/)
- [RabbitMQ 教程](https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html)

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📄 许可证

MIT License

## ✨ 特别说明

### 时间戳处理示例

**后端 C#:**
```csharp
// 存储
public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

// 转换为字符串
var dateStr = CreatedAt.ToDateTimeString("yyyy-MM-dd HH:mm:ss");
```

**前端 JavaScript:**
```javascript
// 转换为可读字符串
import { formatTimestamp } from '@/utils/timestamp'
const dateStr = formatTimestamp(1609459200) // "2021-01-01 00:00:00"
```

### 中间件使用示例

**Shared 项目中定义:**
```csharp
// Shared/Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware { ... }

// Shared/Extensions/ApplicationBuilderExtensions.cs
public static IApplicationBuilder UseCustomMiddlewares(...)
{
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RateLimitingMiddleware>();
    return app;
}
```

**各微服务 Program.cs 中使用:**
```csharp
app.UseCustomMiddlewares(includeRateLimiting: true);
```

---

**🎉 项目已完成！立即运行 `docker-compose up --build` 开始体验！**
