using System.Text;
using System.Text.Json; // For JSON serialization
using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace backend.Infrastructure.Services.MessageBus;

public class RabbitMqMessageBus : IMessageBus, IAsyncDisposable
{
    private readonly ILogger<RabbitMqMessageBus> _logger;
    private readonly ConnectionFactory _factory; // Store the ConnectionFactory
    private IConnection? _connection; // Make nullable
    private IChannel? _channel; // Make nullable
    private bool _disposed;
    private readonly HashSet<string> _declaredExchanges = new HashSet<string>(); // To track declared exchanges
    private readonly JsonSerializerOptions _jsonSerializerOptions; // Add JsonSerializerOptions

    public RabbitMqMessageBus(ILogger<RabbitMqMessageBus> logger, ConnectionFactory factory)
    {
        _logger = logger;
        _factory = factory;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower // Configure snake_case
        };
        _logger.LogInformation("RabbitMqMessageBus initialized with ConnectionFactory.");
    }

    // New InitializeAsync method to establish connection and channel
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
        {
            _logger.LogInformation("RabbitMQ connection and channel already established.");
            return;
        }

        _logger.LogInformation("Attempting to connect to RabbitMQ at {HostName}:{Port}", _factory.HostName, _factory.Port);

        try
        {
            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync();
            _logger.LogInformation("Successfully connected to RabbitMQ and created channel.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ at {HostName}:{Port}", _factory.HostName, _factory.Port);
            throw;
        }
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default) where T : class
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot publish message, RabbitMQMessageBus is disposed.");
            throw new ObjectDisposedException(nameof(RabbitMqMessageBus));
        }

        // Ensure channel is initialized before publishing
        if (_channel == null || !_channel.IsOpen)
        {
            _logger.LogError("RabbitMQ channel is not initialized or is closed. Cannot publish message.");
            throw new InvalidOperationException("RabbitMQ channel is not initialized or is closed.");
        }

        // Only declare the exchange if it hasn't been declared yet
        if (!_declaredExchanges.Contains(exchange))
        {
            await _channel.ExchangeDeclareAsync(exchange: exchange, type: ExchangeType.Topic, durable: true, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            _declaredExchanges.Add(exchange);
        }

        var json = JsonSerializer.Serialize(message, _jsonSerializerOptions);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent // Make messages persistent
        };

        await _channel.BasicPublishAsync(exchange: exchange,
                             routingKey: routingKey,
                             mandatory: false,
                             basicProperties: properties,
                             body: new ReadOnlyMemory<byte>(body), // Use ReadOnlyMemory<byte>
                             cancellationToken: cancellationToken);

        _logger.LogInformation("Published message to exchange '{Exchange}' with routing key '{RoutingKey}'. Message: {Message}", exchange, routingKey, json);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
    }

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _logger.LogInformation("Disposing RabbitMqMessageBus...");

            if (_channel != null && _channel.IsOpen)
            {
                await _channel.CloseAsync();
            }
            _channel?.Dispose(); // Dispose if not null

            if (_connection != null && _connection.IsOpen)
            {
                await _connection.CloseAsync();
            }
            _connection?.Dispose(); // Dispose if not null

            _logger.LogInformation("RabbitMqMessageBus disposed.");
        }

        _disposed = true;
    }
}
