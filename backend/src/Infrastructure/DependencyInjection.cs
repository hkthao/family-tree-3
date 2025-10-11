using backend.Application.Common.Interfaces;
using backend.Infrastructure.Auth;
using backend.Infrastructure.Data;
using backend.Infrastructure.Services;
using backend.Infrastructure.AI.Chat;
using backend.Infrastructure.AI.VectorStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using backend.Application.AI.Embeddings;
using backend.Application.AI.VectorStore;
using backend.Infrastructure.AI.Embeddings;
using backend.Application.AI.Chat;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AISettings;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {

        // Register IAuthProvider based on configuration
        var authProviderType = configuration["AuthProvider"];
        switch (authProviderType)
        {
            case "Auth0":
                services.AddSingleton<IAuthProvider, Auth0Provider>();
                break;
            // Add other providers here as needed
            default:
                throw new InvalidOperationException($"Auth provider '{authProviderType}' is not supported.");
        }

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

        services.AddScoped<IAuthorizationService, AuthorizationService>(); // Added Authorization Service

        services.AddHttpClient(); // Register HttpClient

        // Register Chat Module
        services.Configure<AIChatSettings>(configuration.GetSection(AIChatSettings.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AIChatSettings>>().Value); // Register AIChatSettings as singleton

        services.AddTransient<GeminiChatProvider>(); // Register concrete providers
        services.AddTransient<OpenAIChatProvider>();
        services.AddTransient<LocalChatProvider>();

        services.AddSingleton<IChatProviderFactory, ChatProviderFactory>();
        services.AddScoped<IChatService, ChatService>();

        // Register AI Content Generator
        services.Configure<GeminiSettings>(configuration.GetSection(nameof(GeminiSettings)));
        services.Configure<OpenAISettings>(configuration.GetSection(nameof(OpenAISettings)));
        services.Configure<LocalAISettings>(configuration.GetSection(nameof(LocalAISettings)));
        services.Configure<PineconeSettings>(configuration.GetSection(nameof(PineconeSettings)));
        services.Configure<StorageSettings>(configuration.GetSection(nameof(StorageSettings)));

        // Register Embedding Settings and Providers
        services.Configure<EmbeddingSettings>(configuration.GetSection(EmbeddingSettings.SectionName));
        services.AddScoped<IEmbeddingProvider, OpenAIEmbeddingProvider>();
        services.AddScoped<IEmbeddingProvider, CohereEmbeddingProvider>();
        services.AddScoped<IEmbeddingProvider, LocalEmbeddingProvider>();
                    services.AddScoped<IEmbeddingProviderFactory, EmbeddingProviderFactory>();
        // Register Vector Store
        services.Configure<VectorStoreSettings>(configuration.GetSection(VectorStoreSettings.SectionName));
        services.AddTransient<PineconeVectorStore>();
        services.AddScoped<IVectorStoreFactory, VectorStoreFactory>();

        return services;
    }
}