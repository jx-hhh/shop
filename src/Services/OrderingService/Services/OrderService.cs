using OrderingService.Data;
using OrderingService.Models;
using Shared.DTOs;
using Shared.Messaging;
using System.Security.Claims;

namespace OrderingService.Services;

public interface IOrderService
{
    Task<ApiResponse<OrderDto>> GetByIdAsync(int id, ClaimsPrincipal user);
    Task<ApiResponse<PagedResponse<OrderDto>>> GetMyOrdersAsync(ClaimsPrincipal user, OrderQueryRequest request);
    Task<ApiResponse<OrderDto>> CreateOrderAsync(ClaimsPrincipal user, CreateOrderRequest request);
    Task<ApiResponse<OrderDto>> UpdateStatusAsync(int orderId, string status);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        IEventBus eventBus,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<ApiResponse<OrderDto>> GetByIdAsync(int id, ClaimsPrincipal user)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
        {
            return ApiResponse<OrderDto>.ErrorResponse("Order not found");
        }

        var userId = GetUserId(user);
        if (order.UserId != userId)
        {
            return ApiResponse<OrderDto>.ErrorResponse("Unauthorized");
        }

        var orderDto = MapToDto(order);
        return ApiResponse<OrderDto>.SuccessResponse(orderDto);
    }

    public async Task<ApiResponse<PagedResponse<OrderDto>>> GetMyOrdersAsync(ClaimsPrincipal user, OrderQueryRequest request)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<PagedResponse<OrderDto>>.ErrorResponse("Unauthorized");
        }

        var (orders, totalCount) = await _repository.GetPagedByUserIdAsync(userId, request);

        var pagedResponse = new PagedResponse<OrderDto>
        {
            Items = orders.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return ApiResponse<PagedResponse<OrderDto>>.SuccessResponse(pagedResponse);
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(ClaimsPrincipal user, CreateOrderRequest request)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<OrderDto>.ErrorResponse("Unauthorized");
        }

        // 这里简化处理，实际应该：
        // 1. 从 BasketService 获取购物车内容
        // 2. 从 CatalogService 验证商品库存和价格
        // 3. 扣减库存
        // 为了演示，创建一个简单的订单

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            ShippingAddress = request.ShippingAddress,
            ContactPhone = request.ContactPhone,
            Status = "Pending",
            TotalAmount = 0, // 应该从购物车计算
            Items = new List<OrderItem>() // 应该从购物车获取
        };

        var orderId = await _repository.CreateAsync(order);
        order.Id = orderId;

        // 发布订单创建事件
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = orderId,
            UserId = userId,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount
        };

        await _eventBus.PublishAsync(orderCreatedEvent);
        _logger.LogInformation("Published OrderCreatedEvent for order {OrderId}", orderId);

        var orderDto = MapToDto(order);
        return ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order created successfully");
    }

    public async Task<ApiResponse<OrderDto>> UpdateStatusAsync(int orderId, string status)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order == null)
        {
            return ApiResponse<OrderDto>.ErrorResponse("Order not found");
        }

        var success = await _repository.UpdateStatusAsync(orderId, status);
        if (!success)
        {
            return ApiResponse<OrderDto>.ErrorResponse("Failed to update order status");
        }

        order.Status = status;
        var orderDto = MapToDto(order);

        return ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order status updated");
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{Random.Shared.Next(1000, 9999)}";
    }

    private static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.BookTitle,
                BookAuthor = i.BookAuthor,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}
