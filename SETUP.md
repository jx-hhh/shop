# 网上书店微服务项目 - 环境配置指南

## 前置要求

### 1. 安装 .NET 8 SDK

**Windows (WSL2):**
```bash
# 添加 Microsoft 包存储库
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# 安装 .NET 8 SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# 验证安装
dotnet --version
```

### 2. 安装 Node.js 和 npm (前端开发)

```bash
# 安装 Node.js 18.x LTS
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# 验证安装
node --version
npm --version
```

### 3. 安装 Docker 和 Docker Compose

```bash
# 安装 Docker Desktop for Windows
# 下载地址: https://www.docker.com/products/docker-desktop

# 在 WSL2 中验证
docker --version
docker-compose --version
```

## 项目初始化步骤

### 1. 创建所有 .NET 项目

```bash
# 创建解决方案
dotnet new sln -n OnlineBookstore

# 创建 Shared 类库项目
cd src/Shared
dotnet new classlib -n Shared
cd ../..

# 创建各个微服务项目
cd src/Services/IdentityService
dotnet new webapi -n IdentityService
cd ../../..

cd src/Services/CatalogService
dotnet new webapi -n CatalogService
cd ../../..

cd src/Services/BasketService
dotnet new webapi -n BasketService
cd ../../..

cd src/Services/OrderingService
dotnet new webapi -n OrderingService
cd ../../..

# 创建 Gateway 项目
cd src/Gateway
dotnet new web -n Gateway
cd ../..

# 将所有项目添加到解决方案
dotnet sln add src/Shared/Shared.csproj
dotnet sln add src/Services/IdentityService/IdentityService.csproj
dotnet sln add src/Services/CatalogService/CatalogService.csproj
dotnet sln add src/Services/BasketService/BasketService.csproj
dotnet sln add src/Services/OrderingService/OrderingService.csproj
dotnet sln add src/Gateway/Gateway.csproj
```

### 2. 初始化前端项目

```bash
cd frontend
npm create vite@latest . -- --template vue
npm install
npm install element-plus
npm install pinia
npm install vue-router
npm install axios
npm install @element-plus/icons-vue
cd ..
```

### 3. 启动基础设施服务

```bash
# 启动 RabbitMQ、Redis、Jaeger 等
docker-compose up -d
```

### 4. 运行项目

```bash
# 启动所有后端服务 (使用不同终端窗口)
cd src/Gateway && dotnet run
cd src/Services/IdentityService && dotnet run
cd src/Services/CatalogService && dotnet run
cd src/Services/BasketService && dotnet run
cd src/Services/OrderingService && dotnet run

# 启动前端
cd frontend && npm run dev
```

## 服务端口配置

| 服务 | 端口 | 用途 |
|------|------|------|
| Gateway | 5000 | API 网关 |
| IdentityService | 5001 | 认证服务 |
| CatalogService | 5002 | 商品目录 |
| BasketService | 5003 | 购物车 |
| OrderingService | 5004 | 订单管理 |
| Frontend | 5173 | Vue 前端 |
| RabbitMQ | 5672, 15672 | 消息队列 |
| Redis | 6379 | 缓存 |
| Jaeger | 16686 | 链路追踪 |

## 访问地址

- 前端: http://localhost:5173
- API 网关: http://localhost:5000
- RabbitMQ 管理界面: http://localhost:15672 (guest/guest)
- Jaeger UI: http://localhost:16686
- Swagger (各服务): http://localhost:500[1-4]/swagger

## 下一步

请先安装上述环境，然后运行项目初始化步骤。所有代码文件已经为您准备好了。
