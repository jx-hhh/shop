using Shared.DTOs;
using System.Text.Json;

namespace BasketService.Services;

/// <summary>
/// CatalogService HTTP 客户端接口
/// </summary>
public interface ICatalogServiceClient
{
    Task<BookDto?> GetBookAsync(int bookId);
}

/// <summary>
/// CatalogService HTTP 客户端实现
/// </summary>
public class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CatalogServiceClient(HttpClient httpClient, ILogger<CatalogServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<BookDto?> GetBookAsync(int bookId)
    {
        try
        {
            _logger.LogInformation("Fetching book {BookId} from CatalogService", bookId);

            var response = await _httpClient.GetAsync($"/api/books/{bookId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch book {BookId}. Status: {StatusCode}",
                    bookId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<BookDto>>(content, _jsonOptions);

            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                _logger.LogInformation("Successfully fetched book {BookId}: {Title}",
                    bookId, apiResponse.Data.Title);
                return apiResponse.Data;
            }

            _logger.LogWarning("CatalogService returned unsuccessful response for book {BookId}", bookId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching book {BookId} from CatalogService", bookId);
            return null;
        }
    }
}
