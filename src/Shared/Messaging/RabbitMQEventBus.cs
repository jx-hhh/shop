using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging;

/// <summary>
/// RabbitMQ 事件总线实现
/// </summary>
public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly Dictionary<string, Type> _eventTypes;
    private readonly Dictionary<string, Type> _handlerTypes;

    public RabbitMQEventBus(
        ILogger<RabbitMQEventBus> logger,
        IServiceProvider serviceProvider,
        string hostName = "localhost")
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _eventTypes = new Dictionary<string, Type>();
        _handlerTypes = new Dictionary<string, Type>();

        _logger.LogInformation("Connecting to RabbitMQ at {HostName}", hostName);

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            DispatchConsumersAsync = true
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "bookstore_event_bus", type: ExchangeType.Topic, durable: true);

            _logger.LogInformation("Successfully connected to RabbitMQ and declared exchange 'bookstore_event_bus'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ at {HostName}", hostName);
            throw;
        }
    }

    public async Task PublishAsync<T>(T @event) where T : IntegrationEvent
    {
        var eventName = typeof(T).Name;
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        _logger.LogInformation("Publishing event {EventName} with ID {EventId}. Message: {Message}",
            eventName, @event.Id, message);

        _channel.BasicPublish(
            exchange: "bookstore_event_bus",
            routingKey: eventName,
            basicProperties: null,
            body: body
        );

        _logger.LogInformation("Successfully published event {EventName} with ID {EventId}", eventName, @event.Id);

        await Task.CompletedTask;
    }

    public void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name;
        var handlerType = typeof(TH);

        _logger.LogInformation("Subscribing to event {EventName} with handler {HandlerName}",
            eventName, handlerType.Name);

        if (!_eventTypes.ContainsKey(eventName))
        {
            _eventTypes.Add(eventName, typeof(T));
            _handlerTypes.Add(eventName, handlerType);

            var queueName = $"{eventName}_queue";
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: queueName, exchange: "bookstore_event_bus", routingKey: eventName);

            _logger.LogInformation("Created and bound queue {QueueName} to exchange bookstore_event_bus with routing key {RoutingKey}",
                queueName, eventName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received event {EventName}. Message: {Message}", eventName, message);

                try
                {
                    await ProcessEvent(eventName, message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Successfully processed and acknowledged event {EventName}", eventName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event {EventName}. Message: {Message}", eventName, message);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            _logger.LogInformation("Successfully subscribed to event {EventName} with handler {HandlerName}",
                eventName, handlerType.Name);
        }
        else
        {
            _logger.LogWarning("Already subscribed to event {EventName}", eventName);
        }
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        _logger.LogInformation("Processing event {EventName}", eventName);

        if (_eventTypes.TryGetValue(eventName, out var eventType) &&
            _handlerTypes.TryGetValue(eventName, out var handlerType))
        {
            using var scope = _serviceProvider.CreateScope();
            var @event = JsonSerializer.Deserialize(message, eventType);
            var handler = scope.ServiceProvider.GetService(handlerType);

            if (handler != null && @event != null)
            {
                _logger.LogInformation("Invoking handler {HandlerName} for event {EventName}",
                    handlerType.Name, eventName);

                var method = handlerType.GetMethod("HandleAsync");
                await (Task)method!.Invoke(handler, new[] { @event })!;

                _logger.LogInformation("Handler {HandlerName} completed for event {EventName}",
                    handlerType.Name, eventName);
            }
            else
            {
                _logger.LogWarning("Handler or event is null. Handler: {Handler}, Event: {Event}",
                    handler?.GetType().Name ?? "null", @event?.GetType().Name ?? "null");
            }
        }
        else
        {
            _logger.LogWarning("No handler found for event {EventName}", eventName);
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
