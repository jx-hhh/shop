using Microsoft.Data.Sqlite;

namespace BasketService.Data;

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

        // 创建 Baskets 表
        const string createBasketsTableSql = @"
            CREATE TABLE IF NOT EXISTS Baskets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId TEXT NOT NULL,
                CreatedAt INTEGER NOT NULL,
                UpdatedAt INTEGER
            );";

        using var basketCommand = new SqliteCommand(createBasketsTableSql, connection);
        await basketCommand.ExecuteNonQueryAsync();

        // 创建 BasketItems 表
        const string createItemsTableSql = @"
            CREATE TABLE IF NOT EXISTS BasketItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                BasketId INTEGER NOT NULL,
                BookId INTEGER NOT NULL,
                BookTitle TEXT NOT NULL,
                BookAuthor TEXT NOT NULL,
                BookImageUrl TEXT,
                Price REAL NOT NULL,
                Quantity INTEGER NOT NULL,
                CreatedAt INTEGER NOT NULL,
                UpdatedAt INTEGER,
                FOREIGN KEY (BasketId) REFERENCES Baskets(Id) ON DELETE CASCADE
            );";

        using var itemsCommand = new SqliteCommand(createItemsTableSql, connection);
        await itemsCommand.ExecuteNonQueryAsync();
    }
}
