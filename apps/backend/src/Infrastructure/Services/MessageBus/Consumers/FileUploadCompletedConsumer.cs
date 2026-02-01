using System.Text;
using System.Text.Json;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.MessageBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using backend.Domain.Enums; // NEW
using Microsoft.EntityFrameworkCore; // NEW

namespace backend.Infrastructure.Services.MessageBus.Consumers;

public class FileUploadCompletedConsumer : BackgroundService
{
    private readonly ILogger<FileUploadCompletedConsumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;

    private readonly string _exchangeName = MessageBusConstants.Exchanges.FileUpload;
    private readonly string _queueName = "backend_file_upload_completed_queue"; // Declare _queueName here
    private readonly string _routingKey = MessageBusConstants.RoutingKeys.FileUploadCompleted;

    public FileUploadCompletedConsumer(ILogger<FileUploadCompletedConsumer> logger, IServiceScopeFactory serviceScopeFactory, ConnectionFactory factory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _factory = factory;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FileUploadCompletedConsumer is starting.");

        try
        {
            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey, arguments: null, cancellationToken: cancellationToken);

            _logger.LogInformation("FileUploadCompletedConsumer connected to RabbitMQ. Listening on exchange '{Exchange}' with routing key '{RoutingKey}'.", _exchangeName, _routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not connect to RabbitMQ or declare exchange/queue for FileUploadCompletedConsumer.");
            // Allow the service to try again later if connection fails.
            // Or re-throw if it's a critical startup failure.
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        if (_channel == null)
        {
            _logger.LogError("RabbitMQ channel is null. FileUploadCompletedConsumer cannot execute.");
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
                var eventData = JsonSerializer.Deserialize<FileUploadCompletedEvent>(message, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

                if (eventData != null)
                {
                    await ProcessFileUploadCompletedEvent(eventData, stoppingToken);
                }
                else
                {
                    _logger.LogError("Failed to deserialize FileUploadCompletedEvent: {Message}", message);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for message: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing FileUploadCompletedEvent: {Message}", message);
            }
            finally
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumerTag: _queueName, consumer: consumer);

        _logger.LogInformation("FileUploadCompletedConsumer is waiting for messages.");
    }

    private async Task ProcessFileUploadCompletedEvent(FileUploadCompletedEvent eventData, CancellationToken cancellationToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>(); // Changed from ILocalFileStorageService
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<FileUploadCompletedConsumer>>();

            try
            {
                var familyMedia = await context.FamilyMedia.FindAsync(new object[] { eventData.FileId }, cancellationToken);

                if (familyMedia == null)
                {
                    logger.LogWarning("FamilyMedia with ID {FileId} not found for FileUploadCompletedEvent.", eventData.FileId);
                    return;
                }

                // Delete the locally stored temporary file
                if (!string.IsNullOrEmpty(familyMedia.FilePath) && Path.Exists(familyMedia.FilePath))
                {
                    await fileStorageService.DeleteFileAsync(familyMedia.FilePath, cancellationToken);
                    logger.LogInformation("Deleted temporary local file at {FilePath} for FamilyMedia ID {FileId}.", familyMedia.FilePath, familyMedia.Id);
                }

                familyMedia.FilePath = eventData.FinalFileUrl;
                familyMedia.DeleteHash = eventData.DeleteHash; // Update delete hash if provided

                // Handle entity-specific updates based on RefType and RefId
                if (eventData.RefType.HasValue && eventData.RefId.HasValue)
                {
                    switch (eventData.RefType.Value)
                    {
                        case RefType.UserProfile:
                            var user = await context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Profile!.Id == eventData.RefId.Value, cancellationToken);
                            if (user != null && user.Profile != null)
                            {
                                user.Profile.UpdateAvatar(eventData.FinalFileUrl);
                                logger.LogInformation("UserProfile ID {ProfileId} avatar updated to {FinalFileUrl}.", user.Profile.Id, eventData.FinalFileUrl);
                            }
                            else
                            {
                                logger.LogWarning("UserProfile with ID {ProfileId} not found for avatar update.", eventData.RefId.Value);
                            }
                            break;
                        // Add more cases for other RefTypes as needed
                        default:
                            logger.LogWarning("Unhandled RefType {RefType} for FileId {FileId}.", eventData.RefType.Value, eventData.FileId);
                            break;
                    }
                }

                await context.SaveChangesAsync(cancellationToken);
                logger.LogInformation("FamilyMedia ID {FileId} updated with final URL: {FinalFileUrl}.", familyMedia.Id, eventData.FinalFileUrl);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating FamilyMedia ID {FileId} with final URL.", eventData.FileId);
                // Depending on error, might want to re-queue the message or dead-letter it
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FileUploadCompletedConsumer is stopping.");

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
