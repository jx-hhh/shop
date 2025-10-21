FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["Services/IdentityService/IdentityService.csproj", "Services/IdentityService/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# 还原依赖
RUN dotnet restore "Services/IdentityService/IdentityService.csproj"

# 复制所有文件并构建
COPY . .
WORKDIR "/src/Services/IdentityService"
RUN dotnet build "IdentityService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 创建数据目录
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "IdentityService.dll"]
