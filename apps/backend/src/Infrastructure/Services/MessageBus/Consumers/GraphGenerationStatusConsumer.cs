using System.Text;
using System.Text.Json;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.MessageBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace backend.Infrastructure.Services.MessageBus.Consumers;

public class GraphGenerationStatusConsumer : BackgroundService
{
    private readonly ILogger<GraphGenerationStatusConsumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;

    private readonly string _exchangeName = MessageBusConstants.Exchanges.GraphGeneration;
    private readonly string _queueName = MessageBusConstants.Queues.GraphGenerationStatusQueue;
    private readonly string _routingKey = MessageBusConstants.RoutingKeys.GraphStatusUpdated;

    public GraphGenerationStatusConsumer(ILogger<GraphGenerationStatusConsumer> logger, IServiceScopeFactory serviceScopeFactory, ConnectionFactory factory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _factory = factory;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GraphGenerationStatusConsumer is starting.");

        try
        {
            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey, arguments: null, cancellationToken: cancellationToken);

            _logger.LogInformation("GraphGenerationStatusConsumer connected to RabbitMQ. Listening on exchange '{Exchange}' with routing key '{RoutingKey}'.", _exchangeName, _routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not connect to RabbitMQ or declare exchange/queue for GraphGenerationStatusConsumer.");
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        if (_channel == null)
        {
            _logger.LogError("RabbitMQ channel is null. GraphGenerationStatusConsumer cannot execute.");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received message from RabbitMQ: {Message}", message);

            try
            {
                var eventData = JsonSerializer.Deserialize<GraphGenerationStatusUpdateEvent>(message, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

                if (eventData != null)
                {
                    await ProcessGraphGenerationStatusUpdateEvent(eventData, stoppingToken);
                }
                else
                {
                    _logger.LogError("Failed to deserialize GraphGenerationStatusUpdateEvent: {Message}", message);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for message: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GraphGenerationStatusUpdateEvent: {Message}", message);
            }
            finally
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumerTag: _queueName, consumer: consumer);

        _logger.LogInformation("GraphGenerationStatusConsumer is waiting for messages.");
    }

    private async Task ProcessGraphGenerationStatusUpdateEvent(GraphGenerationStatusUpdateEvent eventData, CancellationToken cancellationToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<GraphGenerationStatusConsumer>>();

            try
            {
                var job = await context.GraphGenerationJobs.FirstOrDefaultAsync(j => j.JobId == eventData.JobId, cancellationToken);

                if (job == null)
                {
                    logger.LogWarning("GraphGenerationJob with JobId {JobId} not found for status update.", eventData.JobId);
                    return;
                }

                job.Status = eventData.Status;
                job.ErrorMessage = eventData.ErrorMessage ?? string.Empty;
                job.OutputFilePath = eventData.OutputFilePath ?? string.Empty;

                if (eventData.Status == "Completed")
                {
                    job.CompletedAt = DateTime.UtcNow;
                }
                else if (eventData.Status == "Failed")
                {
                    job.CompletedAt = DateTime.UtcNow; // Mark as completed (failed)
                }
                // Potentially add 'StartedProcessingAt' if the Python service sends a "Processing" status update

                await context.SaveChangesAsync(cancellationToken);
                logger.LogInformation("GraphGenerationJob {JobId} status updated to {Status}.", job.JobId, job.Status);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating GraphGenerationJob status for JobId {JobId}.", eventData.JobId);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GraphGenerationStatusConsumer is stopping.");

        if (_channel != null && _channel.IsOpen)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
        if (_connection != null && _connection.IsOpen)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
