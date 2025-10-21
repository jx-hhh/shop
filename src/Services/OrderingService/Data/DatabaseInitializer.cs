using Microsoft.Data.Sqlite;

namespace OrderingService.Data;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // 创建 Orders 表
        const string createOrdersTableSql = @"
            CREATE TABLE IF NOT EXISTS Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderNumber TEXT NOT NULL UNIQUE,
                UserId TEXT NOT NULL,
                TotalAmount REAL NOT NULL,
                Status TEXT NOT NULL,
                ShippingAddress TEXT NOT NULL,
                ContactPhone TEXT NOT NULL,
                CreatedAt INTEGER NOT NULL,
                UpdatedAt INTEGER
            );";

        using var ordersCommand = new SqliteCommand(createOrdersTableSql, connection);
        await ordersCommand.ExecuteNonQueryAsync();

        // 创建 OrderItems 表
        const string createItemsTableSql = @"
            CREATE TABLE IF NOT EXISTS OrderItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER NOT NULL,
                BookId INTEGER NOT NULL,
                BookTitle TEXT NOT NULL,
                BookAuthor TEXT NOT NULL,
                Price REAL NOT NULL,
                Quantity INTEGER NOT NULL,
                CreatedAt INTEGER NOT NULL,
                UpdatedAt INTEGER,
                FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
            );";

        using var itemsCommand = new SqliteCommand(createItemsTableSql, connection);
        await itemsCommand.ExecuteNonQueryAsync();
    }
}
