using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration; // To get RabbitMQ configuration

namespace backend.Infrastructure.Services.MessageBus;

/// <summary>
/// Dịch vụ lưu trữ quản lý vòng đời của RabbitMqMessageBus,
/// bao gồm khởi tạo kết nối không đồng bộ và xử lý việc giải phóng tài nguyên.
/// Điều này giúp tránh deadlock khi khởi tạo dịch vụ trong ConfigureServices.
/// </summary>
public class RabbitMqMessageBusHostedService : IHostedService
{
    private readonly ILogger<RabbitMqMessageBusHostedService> _logger;
    private readonly RabbitMqMessageBus _messageBus;
    private readonly IConfiguration _configuration;
    private readonly ConnectionFactory _connectionFactory; // Inject the ConnectionFactory

    public RabbitMqMessageBusHostedService(
        ILogger<RabbitMqMessageBusHostedService> logger,
        RabbitMqMessageBus messageBus,
        IConfiguration configuration,
        ConnectionFactory connectionFactory) // Inject ConnectionFactory
    {
        _logger = logger;
        _messageBus = messageBus;
        _configuration = configuration;
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Bắt đầu dịch vụ lưu trữ và khởi tạo RabbitMqMessageBus không đồng bộ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Một tác vụ biểu thị hoạt động bắt đầu.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMqMessageBusHostedService is starting.");

        // Configure the connection factory properties here, if not already configured in DI
        // This is done to ensure the properties are set before InitializeAsync is called.
        _connectionFactory.HostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
        _connectionFactory.Port = _configuration.GetValue<int?>("RabbitMQ:Port") ?? 5672;
        _connectionFactory.UserName = _configuration["RabbitMQ:UserName"] ?? ConnectionFactory.DefaultUser;
        _connectionFactory.Password = _configuration["RabbitMQ:Password"] ?? ConnectionFactory.DefaultPass;
        _connectionFactory.AutomaticRecoveryEnabled = true;
        _connectionFactory.TopologyRecoveryEnabled = true;


        try
        {
            await _messageBus.InitializeAsync(cancellationToken);
            _logger.LogInformation("RabbitMqMessageBus initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMqMessageBus.");
            // Depending on requirements, you might want to rethrow or just log the error
            // If the message bus is critical, rethrowing would prevent the app from starting.
        }
    }

    /// <summary>
    /// Dừng dịch vụ lưu trữ và giải phóng RabbitMqMessageBus không đồng bộ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Một tác vụ biểu thị hoạt động dừng.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMqMessageBusHostedService is stopping.");
        try
        {
            await _messageBus.DisposeAsync().AsTask(); // Convert ValueTask to Task for await
            _logger.LogInformation("RabbitMqMessageBus disposed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while disposing RabbitMqMessageBus.");
        }
    }
}
