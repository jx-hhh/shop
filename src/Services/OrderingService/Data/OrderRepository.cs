using Dapper;
using Microsoft.Data.Sqlite;
using OrderingService.Models;
using Shared.DTOs;

namespace OrderingService.Data;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<(List<Order> orders, int totalCount)> GetPagedByUserIdAsync(string userId, OrderQueryRequest request);
    Task<int> CreateAsync(Order order);
    Task<bool> UpdateStatusAsync(int orderId, string status);
}

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);

        const string orderSql = "SELECT * FROM Orders WHERE Id = @Id";
        var order = await connection.QueryFirstOrDefaultAsync<Order>(orderSql, new { Id = id });

        if (order == null)
        {
            return null;
        }

        const string itemsSql = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
        var items = await connection.QueryAsync<OrderItem>(itemsSql, new { OrderId = id });
        order.Items = items.ToList();

        return order;
    }

    public async Task<(List<Order> orders, int totalCount)> GetPagedByUserIdAsync(string userId, OrderQueryRequest request)
    {
        using var connection = new SqliteConnection(_connectionString);

        var conditions = new List<string> { "UserId = @UserId" };
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            conditions.Add("Status = @Status");
            parameters.Add("Status", request.Status);
        }

        if (request.StartTime.HasValue)
        {
            conditions.Add("CreatedAt >= @StartTime");
            parameters.Add("StartTime", request.StartTime.Value);
        }

        if (request.EndTime.HasValue)
        {
            conditions.Add("CreatedAt <= @EndTime");
            parameters.Add("EndTime", request.EndTime.Value);
        }

        var whereClause = "WHERE " + string.Join(" AND ", conditions);

        // 获取总数
        var countSql = $"SELECT COUNT(*) FROM Orders {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // 获取分页数据
        var dataSql = $@"
            SELECT * FROM Orders
            {whereClause}
            ORDER BY CreatedAt DESC
            LIMIT @PageSize OFFSET @Skip";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Skip", request.Skip);

        var orders = (await connection.QueryAsync<Order>(dataSql, parameters)).ToList();

        // 加载订单项
        foreach (var order in orders)
        {
            const string itemsSql = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
            var items = await connection.QueryAsync<OrderItem>(itemsSql, new { OrderId = order.Id });
            order.Items = items.ToList();
        }

        return (orders, totalCount);
    }

    public async Task<int> CreateAsync(Order order)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            // 插入订单
            const string orderSql = @"
                INSERT INTO Orders (OrderNumber, UserId, TotalAmount, Status, ShippingAddress, ContactPhone, CreatedAt)
                VALUES (@OrderNumber, @UserId, @TotalAmount, @Status, @ShippingAddress, @ContactPhone, @CreatedAt);
                SELECT last_insert_rowid();";

            var orderId = await connection.ExecuteScalarAsync<int>(orderSql, order, transaction);

            // 插入订单项
            const string itemSql = @"
                INSERT INTO OrderItems (OrderId, BookId, BookTitle, BookAuthor, Price, Quantity, CreatedAt)
                VALUES (@OrderId, @BookId, @BookTitle, @BookAuthor, @Price, @Quantity, @CreatedAt)";

            foreach (var item in order.Items)
            {
                item.OrderId = orderId;
                await connection.ExecuteAsync(itemSql, item, transaction);
            }

            transaction.Commit();
            return orderId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int orderId, string status)
    {
        using var connection = new SqliteConnection(_connectionString);

        const string sql = @"
            UPDATE Orders
            SET Status = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @OrderId";

        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            OrderId = orderId,
            Status = status,
            UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        return affectedRows > 0;
    }
}
