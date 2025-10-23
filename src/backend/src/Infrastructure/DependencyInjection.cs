using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Infrastructure.AI.Chat;
using backend.Infrastructure.AI.Embeddings;
using backend.Infrastructure.AI.TextExtractors;
using backend.Infrastructure.AI.VectorStore;
using backend.Infrastructure.Auth;
using backend.Infrastructure.Data;
using backend.Infrastructure.Files;
using backend.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("FamilyTreeDb"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        }

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // --- Common Services (always registered) ---
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IDateTime, DateTimeService>();

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddAuthorization();

        services.AddScoped<IAuthorizationService, AuthorizationService>();


        // Register Face API Service and configure its HttpClient
        services.AddScoped<IFaceApiService, FaceApiService>(serviceProvider =>
        {
            var configProvider = serviceProvider.GetRequiredService<IConfigProvider>();
            var faceDetectionSettings = configProvider.GetSection<FaceDetectionSettings>();
            var client = new HttpClient { BaseAddress = new Uri(faceDetectionSettings.BaseUrl) };
            return new FaceApiService(client);
        });

        services.AddHttpClient<FaceApiService>(); // Register for HttpClient injection
        {
            var configProvider = serviceProvider.GetRequiredService<IConfigProvider>();
            var faceDetectionSettings = configProvider.GetSection<FaceDetectionSettings>();
            client.BaseAddress = new Uri(faceDetectionSettings.BaseUrl);
        });

        // Register Chat Module

        services.AddTransient<GeminiChatProvider>(); // Register concrete providers
        services.AddTransient<OpenAIChatProvider>();
        services.AddTransient<LocalChatProvider>();

        services.AddSingleton<IChatProviderFactory, ChatProviderFactory>();


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

        return services;
    }
}
