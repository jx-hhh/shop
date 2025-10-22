using Shared.DTOs;
using System.Text.Json;

namespace OrderingService.Services;

/// <summary>
/// BasketService HTTP 客户端接口
/// </summary>
public interface IBasketServiceClient
{
    Task<BasketDto?> GetBasketByUserIdAsync(string userId);
    Task<bool> ClearBasketAsync(string userId);
}

/// <summary>
/// BasketService HTTP 客户端实现
/// </summary>
public class BasketServiceClient : IBasketServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BasketServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public BasketServiceClient(HttpClient httpClient, ILogger<BasketServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<BasketDto?> GetBasketByUserIdAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Fetching basket for user {UserId} from BasketService", userId);

            var response = await _httpClient.GetAsync($"/api/basket/user/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch basket for user {UserId}. Status: {StatusCode}",
                    userId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<BasketDto>>(content, _jsonOptions);

            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                _logger.LogInformation("Successfully fetched basket for user {UserId} with {ItemCount} items",
                    userId, apiResponse.Data.Items.Count);
                return apiResponse.Data;
            }

            _logger.LogWarning("BasketService returned unsuccessful response for user {UserId}", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching basket for user {UserId} from BasketService", userId);
            return null;
        }
    }

    public async Task<bool> ClearBasketAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Clearing basket for user {UserId}", userId);

            var response = await _httpClient.DeleteAsync($"/api/basket/user/{userId}/clear");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to clear basket for user {UserId}. Status: {StatusCode}",
                    userId, response.StatusCode);
                return false;
            }

            _logger.LogInformation("Successfully cleared basket for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing basket for user {UserId}", userId);
            return false;
        }
    }
}
