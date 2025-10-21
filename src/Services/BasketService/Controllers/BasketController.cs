using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace BasketService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BasketController : ControllerBase
{
    private readonly Services.IBasketService _basketService;
    private readonly ILogger<BasketController> _logger;

    public BasketController(Services.IBasketService basketService, ILogger<BasketController> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    /// <summary>
    /// 获取当前用户的购物车
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<BasketDto>>> GetMyBasket()
    {
        _logger.LogInformation("Getting basket for current user");
        var result = await _basketService.GetMyBasketAsync(User);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 添加商品到购物车
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<BasketDto>>> AddItem([FromBody] AddToBasketRequest request)
    {
        _logger.LogInformation("Adding book {BookId} to basket", request.BookId);
        var result = await _basketService.AddItemAsync(User, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 更新购物车商品数量
    /// </summary>
    [HttpPut("items/{itemId}")]
    public async Task<ActionResult<ApiResponse<BasketDto>>> UpdateItemQuantity(
        int itemId,
        [FromBody] UpdateBasketItemRequest request)
    {
        if (itemId != request.BasketItemId)
        {
            return BadRequest(ApiResponse<BasketDto>.ErrorResponse("ID mismatch"));
        }

        _logger.LogInformation("Updating basket item {ItemId} quantity to {Quantity}",
            itemId, request.Quantity);

        var result = await _basketService.UpdateItemQuantityAsync(User, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 从购物车移除商品
    /// </summary>
    [HttpDelete("items/{itemId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveItem(int itemId)
    {
        _logger.LogInformation("Removing basket item {ItemId}", itemId);
        var result = await _basketService.RemoveItemAsync(User, itemId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
