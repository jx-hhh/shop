using Shared.Messaging;

namespace BasketService.EventHandlers;

public class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    private readonly Services.IBasketService _basketService;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(Services.IBasketService basketService, ILogger<OrderCreatedEventHandler> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        _logger.LogInformation("============================================");
        _logger.LogInformation("Handling OrderCreated event - OrderId: {OrderId}, UserId: {UserId}, OrderNumber: {OrderNumber}",
            @event.OrderId, @event.UserId, @event.OrderNumber);

        try
        {
            var result = await _basketService.ClearBasketAsync(@event.UserId);

            if (result.Success)
            {
                _logger.LogInformation("Successfully cleared basket for user: {UserId} after order {OrderNumber} creation",
                    @event.UserId, @event.OrderNumber);
            }
            else
            {
                _logger.LogWarning("Failed to clear basket for user: {UserId} after order {OrderNumber} creation. Error: {Error}",
                    @event.UserId, @event.OrderNumber, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing basket for user: {UserId}, OrderNumber: {OrderNumber}",
                @event.UserId, @event.OrderNumber);
            // 不抛出异常，避免消息重试
        }

        _logger.LogInformation("============================================");
    }
}
