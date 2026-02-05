using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Background;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Family;
using backend.Application.Common.Interfaces.Files;
using backend.Application.Common.Interfaces.Services;
using backend.Application.Common.Interfaces.Services.GraphvizPdfConverter; // NEW: For IGraphvizPdfConverterClient
using backend.Application.Common.Interfaces.Services.LLMGateway; // NEW
using backend.Application.Common.Models.AppSetting;
using backend.Infrastructure.Auth; // For IJwtHelperFactory, JwtHelperFactory, Auth0ClaimsTransformer
using backend.Infrastructure.Data;
using backend.Infrastructure.ExternalApiSettings; // NEW: For GraphvizPdfConverterApiSettings
using backend.Infrastructure.Services;
using backend.Infrastructure.Services.Background;
using backend.Infrastructure.Services.GraphvizPdfConverter; // NEW: For GraphvizPdfConverterClient
using backend.Infrastructure.Services.LLMGateway; // NEW
using backend.Infrastructure.Services.MessageBus; // NEW
using backend.Infrastructure.Services.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client; // NEW

namespace backend.Infrastructure;

/// <summary>
/// Lớp mở rộng để đăng ký các dịch vụ cơ sở hạ tầng (Infrastructure) vào bộ chứa dependency injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký các dịch vụ cần thiết cho tầng cơ sở hạ tầng của ứng dụng.
    /// </summary>
    /// <param name="services">Bộ sưu tập dịch vụ để đăng ký.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>Bộ sưu tập dịch vụ đã được cập nhật.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddHttpContextAccessor(); // Required for ICurrentUser
        services.AddScoped<ICurrentUser, CurrentUser>(); // Register ICurrentUser with its implementation

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                       mySqlOptions => mySqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Register NotificationSettings
        services.Configure<NotificationSettings>(configuration.GetSection(NotificationSettings.SectionName));

        // --- Common Services (always registered) ---
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IDateTime, DateTimeService>();

        // Register RabbitMQ ConnectionFactory
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var hostName = config["RABBITMQ__HOSTNAME"] ?? "localhost";
            var userName = config["RABBITMQ__USERNAME"] ?? "guest";
            var password = config["RABBITMQ__PASSWORD"] ?? "guest";
            var port = config.GetValue<int?>("RABBITMQ__PORT") ?? 5672;

            return new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = port
            };
        });

        // Register IMessageBus with RabbitMqMessageBus implementation
        services.AddSingleton<IMessageBus, RabbitMqMessageBus>();
        services.Configure<NotificationSettings>(configuration.GetSection(NotificationSettings.SectionName));

        // Register IFileStorageService to use LocalDiskFileStorageService
        services.AddScoped<IFileStorageService, LocalDiskFileStorageService>();

        // Register JwtHelperFactory
        services.AddScoped<IJwtHelperFactory, JwtHelperFactory>();

        // Register Background Task Queue
        services.AddSingleton<IBackgroundTaskQueue>(new BackgroundTaskQueue(100)); // Capacity of 100
        services.AddHostedService<QueuedHostedService>();
        services.AddHostedService<backend.Infrastructure.Services.MessageBus.Consumers.FileUploadCompletedConsumer>(); // NEW
        services.AddHostedService<backend.Infrastructure.Services.MessageBus.Consumers.GraphGenerationStatusConsumer>(); // NEW

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddAuthorization();

        services.AddScoped<IAuthorizationService, AuthorizationService>();

        services.AddScoped<IPrivacyService, PrivacyService>();
        services.AddScoped<IMemberRelationshipService, MemberRelationshipService>();
        services.AddScoped<IFamilyTreeService, FamilyTreeService>(); // NEW: Register IFamilyTreeService
        services.AddScoped<IJwtService, JwtService>(); // NEW: Register IJwtService
        services.AddScoped<Domain.Interfaces.IRelationshipGraph, RelationshipGraph>();
        services.AddScoped<Domain.Interfaces.IRelationshipRuleEngine, RelationshipRuleEngine>();
        services.AddScoped<IBackgroundJobService, HangfireJobService>(); // NEW: Register HangfireJobService


        services.AddHttpClient<IFaceApiService, FaceApiService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var faceDetectionSettings = serviceProvider.GetRequiredService<IConfiguration>().GetSection(nameof(FaceDetectionSettings)).Get<FaceDetectionSettings>() ?? new FaceDetectionSettings();
                    if (!string.IsNullOrEmpty(faceDetectionSettings.BaseUrl))
                    {
                        httpClient.BaseAddress = new Uri(faceDetectionSettings.BaseUrl);
                    }
                    else
                    {
                        serviceProvider.GetRequiredService<ILogger<FaceApiService>>().LogWarning("FaceDetection BaseUrl is not configured, falling back to default.");
                    }
                });

        // Register Configuration Provider
        services.AddMemoryCache();

        services.AddTransient<IClaimsTransformation, Auth0ClaimsTransformer>();




        // Register NotificationSettings

        // Register NotificationService
        services.AddScoped<INotificationService, NotificationService>();
        // Add Rate Limiting services
        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.AddPolicy<string, UserRateLimiterPolicy>(RateLimitConstants.PerUserPolicy);
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        // Register LLMGatewaySettings
        services.Configure<LLMGatewaySettings>(configuration.GetSection(LLMGatewaySettings.SectionName));

        // Register LLMGatewayService as a typed HttpClient
        services.AddHttpClient<ILLMGatewayService, LLMGatewayService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var llmGatewaySettings = serviceProvider.GetRequiredService<IOptions<LLMGatewaySettings>>().Value;
                    if (!string.IsNullOrEmpty(llmGatewaySettings.BaseUrl))
                    {
                        httpClient.BaseAddress = new Uri(llmGatewaySettings.BaseUrl);
                    }
                    else
                    {
                        serviceProvider.GetRequiredService<ILogger<LLMGatewayService>>().LogWarning("LLMGateway BaseUrl is not configured, falling back to default.");
                    }
                });

        // Register GraphvizPdfConverterApiSettings
        services.Configure<GraphvizPdfConverterApiSettings>(configuration.GetSection(GraphvizPdfConverterApiSettings.SettingsKey));

        // Register IGraphvizPdfConverterClient as a typed HttpClient
        services.AddHttpClient<IGraphvizPdfConverterClient, GraphvizPdfConverterClient>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var settings = serviceProvider.GetRequiredService<IOptions<GraphvizPdfConverterApiSettings>>().Value;
                    if (!string.IsNullOrEmpty(settings.BaseUrl))
                    {
                        httpClient.BaseAddress = new Uri(settings.BaseUrl);
                    }
                    else
                    {
                        serviceProvider.GetRequiredService<ILogger<GraphvizPdfConverterClient>>().LogWarning("GraphvizPdfConverterApi BaseUrl is not configured.");
                    }
                });

        return services;
    }




}

