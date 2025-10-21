using Microsoft.Data.Sqlite;

namespace CatalogService.Data;

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

        // 创建 Books 表
        const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS Books (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Author TEXT NOT NULL,
                ISBN TEXT NOT NULL,
                Description TEXT NOT NULL,
                Price REAL NOT NULL,
                Stock INTEGER NOT NULL,
                Category TEXT NOT NULL,
                ImageUrl TEXT,
                CreatedAt INTEGER NOT NULL,
                UpdatedAt INTEGER
            );";

        using var command = new SqliteCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();

        // 检查是否需要添加种子数据
        const string countSql = "SELECT COUNT(*) FROM Books";
        using var countCommand = new SqliteCommand(countSql, connection);
        var count = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

        if (count == 0)
        {
            await SeedDataAsync(connection);
        }
    }

    private async Task SeedDataAsync(SqliteConnection connection)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        const string insertSql = @"
            INSERT INTO Books (Title, Author, ISBN, Description, Price, Stock, Category, ImageUrl, CreatedAt)
            VALUES
            ('Clean Code', 'Robert C. Martin', '9780132350884', 'A Handbook of Agile Software Craftsmanship', 42.99, 50, 'Programming', 'https://images.example.com/clean-code.jpg', @Timestamp),
            ('Design Patterns', 'Gang of Four', '9780201633610', 'Elements of Reusable Object-Oriented Software', 54.99, 30, 'Programming', 'https://images.example.com/design-patterns.jpg', @Timestamp),
            ('The Pragmatic Programmer', 'Andrew Hunt', '9780135957059', 'Your Journey To Mastery', 49.99, 40, 'Programming', 'https://images.example.com/pragmatic-programmer.jpg', @Timestamp),
            ('Introduction to Algorithms', 'Thomas H. Cormen', '9780262033848', 'A comprehensive textbook on algorithms', 89.99, 25, 'Computer Science', 'https://images.example.com/algorithms.jpg', @Timestamp),
            ('You Don''t Know JS', 'Kyle Simpson', '9781491924464', 'Deep dive into JavaScript', 34.99, 60, 'Programming', 'https://images.example.com/ydkjs.jpg', @Timestamp),
            ('Refactoring', 'Martin Fowler', '9780134757599', 'Improving the Design of Existing Code', 47.99, 35, 'Programming', 'https://images.example.com/refactoring.jpg', @Timestamp),
            ('The Art of Computer Programming', 'Donald Knuth', '9780201896831', 'Fundamental Algorithms', 79.99, 20, 'Computer Science', 'https://images.example.com/taocp.jpg', @Timestamp),
            ('Code Complete', 'Steve McConnell', '9780735619678', 'A Practical Handbook of Software Construction', 44.99, 45, 'Programming', 'https://images.example.com/code-complete.jpg', @Timestamp),
            ('Head First Design Patterns', 'Eric Freeman', '9780596007126', 'A Brain-Friendly Guide', 39.99, 55, 'Programming', 'https://images.example.com/hfdp.jpg', @Timestamp),
            ('Domain-Driven Design', 'Eric Evans', '9780321125217', 'Tackling Complexity in the Heart of Software', 59.99, 28, 'Software Architecture', 'https://images.example.com/ddd.jpg', @Timestamp);";

        using var command = new SqliteCommand(insertSql, connection);
        command.Parameters.AddWithValue("@Timestamp", timestamp);
        await command.ExecuteNonQueryAsync();
    }
}
