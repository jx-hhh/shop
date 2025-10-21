FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["Services/BasketService/BasketService.csproj", "Services/BasketService/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# 还原依赖
RUN dotnet restore "Services/BasketService/BasketService.csproj"

# 复制所有文件并构建
COPY . .
WORKDIR "/src/Services/BasketService"
RUN dotnet build "BasketService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasketService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 创建数据目录
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "BasketService.dll"]
