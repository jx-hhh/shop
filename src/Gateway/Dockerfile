FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["src/Gateway/Gateway.csproj", "src/Gateway/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

# 还原依赖
RUN dotnet restore "src/Gateway/Gateway.csproj"

# 复制所有文件并构建
COPY . .
WORKDIR "/src/src/Gateway"
RUN dotnet build "Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Gateway.dll"]
