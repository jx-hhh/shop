using BasketService.Data;
using BasketService.Models;
using Shared.DTOs;
using System.Security.Claims;

namespace BasketService.Services;

public interface IBasketService
{
    Task<ApiResponse<BasketDto>> GetMyBasketAsync(ClaimsPrincipal user);
    Task<ApiResponse<BasketDto>> GetBasketByUserIdAsync(string userId);
    Task<ApiResponse<BasketDto>> AddItemAsync(ClaimsPrincipal user, AddToBasketRequest request);
    Task<ApiResponse<BasketDto>> UpdateItemQuantityAsync(ClaimsPrincipal user, UpdateBasketItemRequest request);
    Task<ApiResponse<bool>> RemoveItemAsync(ClaimsPrincipal user, int itemId);
    Task<ApiResponse<bool>> ClearBasketAsync(string userId);
}

public class BasketService : IBasketService
{
    private readonly IBasketRepository _repository;
    private readonly ICatalogServiceClient _catalogServiceClient;
    private readonly ILogger<BasketService> _logger;

    public BasketService(
        IBasketRepository repository,
        ICatalogServiceClient catalogServiceClient,
        ILogger<BasketService> logger)
    {
        _repository = repository;
        _catalogServiceClient = catalogServiceClient;
        _logger = logger;
    }

    public async Task<ApiResponse<BasketDto>> GetMyBasketAsync(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<BasketDto>.ErrorResponse("Unauthorized");
        }

        var basket = await GetOrCreateBasketAsync(userId);
        var basketDto = MapToDto(basket);

        return ApiResponse<BasketDto>.SuccessResponse(basketDto);
    }

    public async Task<ApiResponse<BasketDto>> GetBasketByUserIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<BasketDto>.ErrorResponse("UserId is required");
        }

        var basket = await GetOrCreateBasketAsync(userId);
        var basketDto = MapToDto(basket);

        return ApiResponse<BasketDto>.SuccessResponse(basketDto);
    }

    public async Task<ApiResponse<BasketDto>> AddItemAsync(ClaimsPrincipal user, AddToBasketRequest request)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<BasketDto>.ErrorResponse("Unauthorized");
        }

        var basket = await GetOrCreateBasketAsync(userId);

        // 检查商品是否已在购物车中
        var existingItem = basket.Items.FirstOrDefault(i => i.BookId == request.BookId);
        if (existingItem != null)
        {
            // 更新数量
            existingItem.Quantity += request.Quantity;
            await _repository.UpdateItemAsync(existingItem);
        }
        else
        {
            // 从 CatalogService 获取图书信息
            var book = await _catalogServiceClient.GetBookAsync(request.BookId);
            if (book == null)
            {
                return ApiResponse<BasketDto>.ErrorResponse($"Book with ID {request.BookId} not found");
            }

            var newItem = new BasketItem
            {
                BasketId = basket.Id,
                BookId = request.BookId,
                BookTitle = book.Title,
                BookAuthor = book.Author,
                BookImageUrl = book.ImageUrl,
                Price = book.Price,
                Quantity = request.Quantity
            };

            await _repository.AddItemAsync(newItem);
            basket.Items.Add(newItem);
        }

        var basketDto = MapToDto(basket);
        return ApiResponse<BasketDto>.SuccessResponse(basketDto, "Item added to basket");
    }

    public async Task<ApiResponse<BasketDto>> UpdateItemQuantityAsync(ClaimsPrincipal user, UpdateBasketItemRequest request)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<BasketDto>.ErrorResponse("Unauthorized");
        }

        var item = await _repository.GetItemByIdAsync(request.BasketItemId);
        if (item == null)
        {
            return ApiResponse<BasketDto>.ErrorResponse("Item not found");
        }

        if (request.Quantity <= 0)
        {
            return ApiResponse<BasketDto>.ErrorResponse("Quantity must be greater than 0");
        }

        item.Quantity = request.Quantity;
        await _repository.UpdateItemAsync(item);

        var basket = await _repository.GetByUserIdAsync(userId);
        if (basket == null)
        {
            return ApiResponse<BasketDto>.ErrorResponse("Basket not found");
        }

        var basketDto = MapToDto(basket);
        return ApiResponse<BasketDto>.SuccessResponse(basketDto, "Item quantity updated");
    }

    public async Task<ApiResponse<bool>> RemoveItemAsync(ClaimsPrincipal user, int itemId)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<bool>.ErrorResponse("Unauthorized");
        }

        var success = await _repository.RemoveItemAsync(itemId);
        if (!success)
        {
            return ApiResponse<bool>.ErrorResponse("Failed to remove item");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Item removed from basket");
    }

    public async Task<ApiResponse<bool>> ClearBasketAsync(string userId)
    {
        var success = await _repository.ClearAsync(userId);
        if (!success)
        {
            _logger.LogWarning("Failed to clear basket for user: {UserId}", userId);
            return ApiResponse<bool>.ErrorResponse("Failed to clear basket");
        }

        _logger.LogInformation("Basket cleared for user: {UserId}", userId);
        return ApiResponse<bool>.SuccessResponse(true, "Basket cleared");
    }

    private async Task<Basket> GetOrCreateBasketAsync(string userId)
    {
        var basket = await _repository.GetByUserIdAsync(userId);
        if (basket == null)
        {
            basket = new Basket { UserId = userId };
            basket.Id = await _repository.CreateAsync(basket);
        }

        return basket;
    }

    private static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;
    }

    private static BasketDto MapToDto(Basket basket)
    {
        return new BasketDto
        {
            Id = basket.Id,
            UserId = basket.UserId,
            Items = basket.Items.Select(i => new BasketItemDto
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.BookTitle,
                BookAuthor = i.BookAuthor,
                BookImageUrl = i.BookImageUrl,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList(),
            CreatedAt = basket.CreatedAt,
            UpdatedAt = basket.UpdatedAt
        };
    }
}
