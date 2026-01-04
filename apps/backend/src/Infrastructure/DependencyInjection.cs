using System.Net.Http.Headers;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Common.Configurations;
using backend.Infrastructure.Auth; // For IJwtHelperFactory, JwtHelperFactory, Auth0ClaimsTransformer
using backend.Infrastructure.Data;
using backend.Infrastructure.Novu;
using backend.Infrastructure.Services;
using backend.Infrastructure.Services.Background;
using backend.Infrastructure.Services.RateLimiting;
using FamilyTree.Infrastructure; // For ImgbbSettings
using FamilyTree.Infrastructure.Services; // For ImgbbImageUploadService
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novu;


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

        // --- Common Services (always registered) ---
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IDateTime, DateTimeService>();
        // Register NotificationSettings
        services.Configure<NotificationSettings>(configuration.GetSection(NotificationSettings.SectionName));

        // Register N8nSettings
        services.Configure<N8nSettings>(configuration.GetSection(N8nSettings.SectionName));

        // Register ImgbbSettings
        services.Configure<ImgbbSettings>(configuration.GetSection(ImgbbSettings.SectionName));

        // Register ImgbbImageUploadService and configure its HttpClient
        services.AddHttpClient<IImageUploadService, ImgbbImageUploadService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var imgbbSettings = serviceProvider.GetRequiredService<IOptions<ImgbbSettings>>().Value;
                    httpClient.BaseAddress = new Uri(imgbbSettings.BaseUrl);
                });

        // Register ImgurSettings (đã có)
        services.Configure<ImgurSettings>(configuration.GetSection(nameof(ImgurSettings)));

        // Register CloudinarySettings
        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));


        // Register FileStorageSettings
        services.Configure<FileStorageSettings>(configuration.GetSection(FileStorageSettings.SectionName));
        services.Configure<CloudflareR2Settings>(configuration.GetSection(CloudflareR2Settings.SectionName));



        // Configure HttpClient for ImgurFileStorageService
        services.AddHttpClient<ImgurFileStorageService>(httpClient =>
        {
            var serviceProvider = services.BuildServiceProvider(); // Temporarily build provider to get settings
            var imgurSettings = serviceProvider.GetRequiredService<IOptions<ImgurSettings>>().Value;
            httpClient.BaseAddress = new Uri("https://api.imgur.com/3/");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", imgurSettings.ClientId);
        });

        // Register N8nService if it's used by N8nFileStorageService
        services.AddScoped<IN8nService, N8nService>();




        // Dynamically register IFileStorageService based on configuration
        services.AddScoped<IFileStorageService>(serviceProvider =>
        {
            var fileStorageSettings = serviceProvider.GetRequiredService<IOptions<FileStorageSettings>>().Value;

            return fileStorageSettings.Provider switch
            {
                "Imgur" => serviceProvider.GetRequiredService<ImgurFileStorageService>(), // Get already configured ImgurFileStorageService
                "CloudflareR2" => new CloudflareR2FileStorageService(
                                        serviceProvider.GetRequiredService<IOptions<CloudflareR2Settings>>(),
                                        serviceProvider.GetRequiredService<ILogger<CloudflareR2FileStorageService>>()
                                    ),
                "Cloudinary" => new CloudinaryFileStorageService(
                                        serviceProvider.GetRequiredService<IOptions<CloudinarySettings>>(),
                                        serviceProvider.GetRequiredService<ILogger<CloudinaryFileStorageService>>()
                                    ),
                "N8n" => new N8nFileStorageService(serviceProvider.GetRequiredService<IN8nService>()),
                _ => throw new ArgumentException($"Unknown file storage provider: {fileStorageSettings.Provider}")
            };
        });

        // Register JwtHelperFactory
        services.AddScoped<IJwtHelperFactory, JwtHelperFactory>();

        // Register Background Task Queue
        services.AddSingleton<IBackgroundTaskQueue>(new BackgroundTaskQueue(100)); // Capacity of 100
        services.AddHostedService<QueuedHostedService>();

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddAuthorization();

        services.AddScoped<IAuthorizationService, AuthorizationService>();

        services.AddScoped<IPrivacyService, PrivacyService>();
        services.AddScoped<IMemberRelationshipService, MemberRelationshipService>();
        services.AddScoped<IFamilyTreeService, FamilyTreeService>(); // NEW: Register IFamilyTreeService
        services.AddScoped<IJwtService, JwtService>(); // NEW: Register IJwtService
        services.AddScoped<Domain.Interfaces.IRelationshipGraph, RelationshipGraph>();
        services.AddScoped<Domain.Interfaces.IRelationshipRuleEngine, RelationshipRuleEngine>();


        // Register Face API Service and configure its HttpClient
        services.AddScoped<IFaceApiService, FaceApiService>(serviceProvider =>
        {
            var faceDetectionSettings = configuration.GetSection(nameof(FaceDetectionSettings)).Get<FaceDetectionSettings>() ?? new FaceDetectionSettings();
            var logger = serviceProvider.GetRequiredService<ILogger<FaceApiService>>(); // Get the logger
            var client = new HttpClient { BaseAddress = new Uri(faceDetectionSettings.BaseUrl) };
            return new FaceApiService(logger, client); // Pass both logger and client
        });

        services.AddHttpClient<FaceApiService>(); // Register for HttpClient injection

        // Register Configuration Provider
        services.AddMemoryCache();

        services.AddTransient<IClaimsTransformation, Auth0ClaimsTransformer>();

        // Register n8n Service
        services.AddScoped<IN8nService, N8nService>();
        services.AddScoped<IAiChatService, AiChatService>();
        services.AddScoped<IAiGenerateService, AiGenerateService>();
        // Register Notification Provider Factory
        services.AddScoped<INotificationProviderFactory, NotificationProviderFactory>();

        // If you want to use N8nFileStorageService instead, comment out the Imgur registration above
        // and uncomment the line below.
        // services.AddScoped<IFileStorageService, N8nFileStorageService>();

        // Register Novu services
        services.AddNovuServices(configuration);
        // Add Rate Limiting services
        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.AddPolicy<string, UserRateLimiterPolicy>(RateLimitConstants.PerUserPolicy);
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        // Register VoiceAISettings
        services.Configure<VoiceAISettings>(configuration.GetSection(VoiceAISettings.SectionName));

        // Register VoiceAIService as a typed HttpClient
        services.AddHttpClient<IVoiceAIService, VoiceAIService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var voiceAISettings = serviceProvider.GetRequiredService<IOptions<VoiceAISettings>>().Value;
                    if (!string.IsNullOrEmpty(voiceAISettings.BaseUrl))
                    {
                        httpClient.BaseAddress = new Uri(voiceAISettings.BaseUrl);
                    }
                    else
                    {
                        serviceProvider.GetRequiredService<ILogger<VoiceAIService>>().LogWarning("VoiceAISettings BaseUrl is not configured.");
                    }
                });

        return services;
    }

    /// <summary>
    /// Adds Novu related services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddNovuServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Configure and bind Novu settings from appsettings.json
        var novuSettings = new NovuSettings();
        configuration.GetSection(NovuSettings.SectionName).Bind(novuSettings);
        services.AddSingleton(Options.Create(novuSettings));

        // 2. Register NovuSDK
        services.AddSingleton(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<NovuSettings>>().Value;
            var logger = provider.GetRequiredService<ILogger<NovuSDK>>();

            if (string.IsNullOrEmpty(settings.ApiKey))
            {
                throw new ArgumentNullException(nameof(settings.ApiKey), "Novu API Key is not configured. Please check NovuSettings in appsettings.json or environment variables.");
            }

            // logger.LogInformation("NovuSDK secretKey: ", settings.ApiKey);
            return new NovuSDK(secretKey: settings.ApiKey);
        });

        // 3. Register NovuNotificationProvider as INotificationProvider
        services.AddScoped<INotificationProvider, NovuNotificationProvider>();

        return services;
    }


}

