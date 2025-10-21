using BasketService.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BasketService.Data;

public interface IBasketRepository
{
    Task<Basket?> GetByUserIdAsync(string userId);
    Task<int> CreateAsync(Basket basket);
    Task<bool> ClearAsync(string userId);
    Task<BasketItem?> GetItemByIdAsync(int itemId);
    Task<int> AddItemAsync(BasketItem item);
    Task<bool> UpdateItemAsync(BasketItem item);
    Task<bool> RemoveItemAsync(int itemId);
}

public class BasketRepository : IBasketRepository
{
    private readonly string _connectionString;

    public BasketRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<Basket?> GetByUserIdAsync(string userId)
    {
        using var connection = new SqliteConnection(_connectionString);

        const string basketSql = "SELECT * FROM Baskets WHERE UserId = @UserId";
        var basket = await connection.QueryFirstOrDefaultAsync<Basket>(basketSql, new { UserId = userId });

        if (basket == null)
        {
            return null;
        }

        const string itemsSql = "SELECT * FROM BasketItems WHERE BasketId = @BasketId";
        var items = await connection.QueryAsync<BasketItem>(itemsSql, new { BasketId = basket.Id });
        basket.Items = items.ToList();

        return basket;
    }

    public async Task<int> CreateAsync(Basket basket)
    {
        using var connection = new SqliteConnection(_connectionString);

        const string sql = @"
            INSERT INTO Baskets (UserId, CreatedAt)
            VALUES (@UserId, @CreatedAt);
            SELECT last_insert_rowid();";

        return await connection.ExecuteScalarAsync<int>(sql, basket);
    }

    public async Task<bool> ClearAsync(string userId)
    {
        using var connection = new SqliteConnection(_connectionString);

        // 获取购物车 ID
        const string getBasketIdSql = "SELECT Id FROM Baskets WHERE UserId = @UserId";
        var basketId = await connection.ExecuteScalarAsync<int?>(getBasketIdSql, new { UserId = userId });

        if (basketId == null)
        {
            return false;
        }

        // 删除所有购物车项
        const string deleteItemsSql = "DELETE FROM BasketItems WHERE BasketId = @BasketId";
        await connection.ExecuteAsync(deleteItemsSql, new { BasketId = basketId.Value });

        return true;
    }

    public async Task<BasketItem?> GetItemByIdAsync(int itemId)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = "SELECT * FROM BasketItems WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<BasketItem>(sql, new { Id = itemId });
    }

    public async Task<int> AddItemAsync(BasketItem item)
    {
        using var connection = new SqliteConnection(_connectionString);

        const string sql = @"
            INSERT INTO BasketItems (BasketId, BookId, BookTitle, BookAuthor, BookImageUrl, Price, Quantity, CreatedAt)
            VALUES (@BasketId, @BookId, @BookTitle, @BookAuthor, @BookImageUrl, @Price, @Quantity, @CreatedAt);
            SELECT last_insert_rowid();";

        return await connection.ExecuteScalarAsync<int>(sql, item);
    }

    public async Task<bool> UpdateItemAsync(BasketItem item)
    {
        using var connection = new SqliteConnection(_connectionString);

        const string sql = @"
            UPDATE BasketItems
            SET Quantity = @Quantity,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        item.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var affectedRows = await connection.ExecuteAsync(sql, item);
        return affectedRows > 0;
    }

    public async Task<bool> RemoveItemAsync(int itemId)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = "DELETE FROM BasketItems WHERE Id = @Id";
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = itemId });
        return affectedRows > 0;
    }
}
