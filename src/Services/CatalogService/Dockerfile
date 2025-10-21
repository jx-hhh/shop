FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["src/Services/CatalogService/CatalogService.csproj", "src/Services/CatalogService/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

# 还原依赖
RUN dotnet restore "src/Services/CatalogService/CatalogService.csproj"

# 复制所有文件并构建
COPY . .
WORKDIR "/src/src/Services/CatalogService"
RUN dotnet build "CatalogService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatalogService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 创建数据目录
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "CatalogService.dll"]
