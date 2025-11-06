using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Infrastructure.AI.Embeddings;
using backend.Infrastructure.AI.TextExtractors;
using backend.Infrastructure.AI.VectorStore;
using backend.Infrastructure.Auth;
using backend.Infrastructure.Data;
using backend.Infrastructure.Data.Interceptors;
using backend.Infrastructure.Files;
using backend.Infrastructure.Novu;
using backend.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
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
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddHttpContextAccessor(); // Required for ICurrentUser
        services.AddScoped<ICurrentUser, CurrentUser>(); // Register ICurrentUser with its implementation

        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                options.UseInMemoryDatabase("FamilyTreeDb")
                           .AddInterceptors(
                           serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>(),
                           serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()))
                           ;
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                           mySqlOptions => mySqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null))
                       .AddInterceptors(
                           serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>(),
                           serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()))
                           ;
        }

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // --- Common Services (always registered) ---
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IDateTime, DateTimeService>();
        // Register NotificationSettings
        services.Configure<NotificationSettings>(configuration.GetSection(NotificationSettings.SectionName));
        services.AddScoped<IGlobalSearchService, GlobalSearchService>();

        // Register Background Task Queue
        services.AddSingleton<IBackgroundTaskQueue>(new BackgroundTaskQueue(100)); // Capacity of 100
        services.AddHostedService<QueuedHostedService>();

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddAuthorization();

        services.AddScoped<IAuthorizationService, AuthorizationService>();

        // Register Face API Service and configure its HttpClient
        services.AddScoped<IFaceApiService, FaceApiService>(serviceProvider =>
        {
            var configProvider = serviceProvider.GetRequiredService<IConfigProvider>();
            var faceDetectionSettings = configProvider.GetSection<FaceDetectionSettings>();
            var logger = serviceProvider.GetRequiredService<ILogger<FaceApiService>>(); // Get the logger
            var client = new HttpClient { BaseAddress = new Uri(faceDetectionSettings.BaseUrl) };
            return new FaceApiService(logger, client); // Pass both logger and client
        });

        services.AddHttpClient<FaceApiService>(); // Register for HttpClient injection



        // Register AI Content Generator

        // Register Embedding Settings and Providers
        services.AddTransient<OpenAIEmbeddingProvider>();
        services.AddTransient<CohereEmbeddingProvider>();
        services.AddTransient<LocalEmbeddingProvider>();
        services.AddScoped<IEmbeddingProviderFactory, EmbeddingProviderFactory>();
        // Register Vector Store
        services.AddTransient<InMemoryVectorStore>();
        services.AddTransient<PineconeVectorStore>();
        services.AddTransient<QdrantVectorStore>();
        services.AddScoped<IVectorStoreFactory, VectorStoreFactory>();

        services.AddTransient<PdfTextExtractor>();
        services.AddTransient<TxtTextExtractor>();
        services.AddTransient<MdTextExtractor>();
        services.AddScoped<IFileTextExtractorFactory, FileTextExtractorFactory>();

        // Register Configuration Provider
        services.AddMemoryCache();
        services.AddScoped<IConfigProvider, Application.Common.Services.ConfigProvider>();

        // Register File Storage
        services.AddTransient<LocalFileStorage>();
        services.AddTransient<S3FileStorage>();
        services.AddTransient<CloudinaryFileStorage>();
        services.AddSingleton<IFileStorageFactory, FileStorageFactory>();

        services.AddTransient<IClaimsTransformation, Auth0ClaimsTransformer>();

        // Register n8n Service
        services.AddScoped<IN8nService, N8nService>();

        // Register Notification Provider Factory
        services.AddScoped<INotificationProviderFactory, NotificationProviderFactory>();

        // Add Novu services
        services.AddNovuServices(configuration);

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
        services.AddSingleton<NovuSDK>(provider =>
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
