using CatalogService.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using Shared.DTOs;

namespace CatalogService.Data;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(int id);
    Task<(List<Book> books, int totalCount)> GetPagedAsync(SearchBooksRequest request);
    Task<List<Book>> GetAllAsync();
    Task<int> CreateAsync(Book book);
    Task<bool> UpdateAsync(Book book);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateStockAsync(int bookId, int quantity);
}

public class BookRepository : IBookRepository
{
    private readonly string _connectionString;

    public BookRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = "SELECT * FROM Books WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Book>(sql, new { Id = id });
    }

    public async Task<(List<Book> books, int totalCount)> GetPagedAsync(SearchBooksRequest request)
    {
        using var connection = new SqliteConnection(_connectionString);

        // 构建查询条件
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            conditions.Add("(Title LIKE @Keyword OR Author LIKE @Keyword OR ISBN LIKE @Keyword)");
            parameters.Add("Keyword", $"%{request.Keyword}%");
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            conditions.Add("Category = @Category");
            parameters.Add("Category", request.Category);
        }

        if (request.MinPrice.HasValue)
        {
            conditions.Add("Price >= @MinPrice");
            parameters.Add("MinPrice", request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            conditions.Add("Price <= @MaxPrice");
            parameters.Add("MaxPrice", request.MaxPrice.Value);
        }

        var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";

        // 获取总数
        var countSql = $"SELECT COUNT(*) FROM Books {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // 获取分页数据
        var dataSql = $@"
            SELECT * FROM Books
            {whereClause}
            ORDER BY CreatedAt DESC
            LIMIT @PageSize OFFSET @Skip";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Skip", request.Skip);

        var books = (await connection.QueryAsync<Book>(dataSql, parameters)).ToList();

        return (books, totalCount);
    }

    public async Task<List<Book>> GetAllAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = "SELECT * FROM Books ORDER BY CreatedAt DESC";
        var result = await connection.QueryAsync<Book>(sql);
        return result.ToList();
    }

    public async Task<int> CreateAsync(Book book)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = @"
            INSERT INTO Books (Title, Author, ISBN, Description, Price, Stock, Category, ImageUrl, CreatedAt)
            VALUES (@Title, @Author, @ISBN, @Description, @Price, @Stock, @Category, @ImageUrl, @CreatedAt);
            SELECT last_insert_rowid();";

        return await connection.ExecuteScalarAsync<int>(sql, book);
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = @"
            UPDATE Books
            SET Title = @Title,
                Author = @Author,
                ISBN = @ISBN,
                Description = @Description,
                Price = @Price,
                Stock = @Stock,
                Category = @Category,
                ImageUrl = @ImageUrl,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        book.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var affectedRows = await connection.ExecuteAsync(sql, book);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = "DELETE FROM Books WHERE Id = @Id";
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }

    public async Task<bool> UpdateStockAsync(int bookId, int quantity)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = @"
            UPDATE Books
            SET Stock = Stock + @Quantity,
                UpdatedAt = @UpdatedAt
            WHERE Id = @BookId";

        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            BookId = bookId,
            Quantity = quantity,
            UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        return affectedRows > 0;
    }
}
