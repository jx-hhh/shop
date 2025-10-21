using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderingService.Services;
using Shared.DTOs;

namespace OrderingService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// 获取订单详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(int id)
    {
        _logger.LogInformation("Getting order {OrderId}", id);
        var result = await _orderService.GetByIdAsync(id, User);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 获取我的订单列表
    /// </summary>
    [HttpGet("my-orders")]
    public async Task<ActionResult<ApiResponse<PagedResponse<OrderDto>>>> GetMyOrders([FromQuery] OrderQueryRequest request)
    {
        _logger.LogInformation("Getting my orders");
        var result = await _orderService.GetMyOrdersAsync(User, request);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 创建订单
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation("Creating order");
        var result = await _orderService.CreateOrderAsync(User, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// 更新订单状态（管理员）
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        if (id != request.OrderId)
        {
            return BadRequest(ApiResponse<OrderDto>.ErrorResponse("ID mismatch"));
        }

        _logger.LogInformation("Updating order {OrderId} status to {Status}", id, request.Status);
        var result = await _orderService.UpdateStatusAsync(id, request.Status);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
