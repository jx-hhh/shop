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
        _logger.LogInformation("Handling OrderCreated event for user: {UserId}, order: {OrderId}",
            @event.UserId, @event.OrderId);

        try
        {
            await _basketService.ClearBasketAsync(@event.UserId);
            _logger.LogInformation("Basket cleared for user: {UserId} after order creation", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing basket for user: {UserId}", @event.UserId);
            throw;
        }
    }
}
