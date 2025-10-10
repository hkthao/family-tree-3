using backend.Application.Common.Interfaces;
using backend.Infrastructure.Auth;
using backend.Infrastructure.Data;
using backend.Infrastructure.Services;
using backend.Infrastructure.AI.Chat;
using backend.Infrastructure.AI.VectorStore;
using backend.Application.AI.ContentGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using backend.Application.AI.Embeddings;
using backend.Application.AI.VectorStore;
using backend.Infrastructure.AI.Embeddings;
using backend.Application.AI.Chat;

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

        // Register Chat Module
        services.Configure<AIChatSettings>(configuration.GetSection(AIChatSettings.SectionName));
        services.AddTransient<IChatProvider, GeminiProvider>();
        services.AddTransient<IChatProvider, OpenAIProvider>();
        services.AddTransient<IChatProvider, LocalAIProvider>();
        services.AddSingleton<IChatProviderFactory, ChatProviderFactory>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IEmbeddingGenerator, EmbeddingGenerator>();

        // Register AI Content Generator
        services.Configure<AIContentGeneratorSettings>(configuration.GetSection(AIContentGeneratorSettings.SectionName));

        // Register Embedding Settings
        services.Configure<EmbeddingSettings>(configuration.GetSection(EmbeddingSettings.SectionName));

        // Register Vector Store
        services.Configure<VectorStoreSettings>(configuration.GetSection(VectorStoreSettings.SectionName));
        services.AddTransient<PineconeVectorStore>();

        services.AddScoped<IVectorStoreFactory, VectorStoreFactory>();
        services.AddScoped<IVectorStore>(sp => sp.GetRequiredService<IVectorStoreFactory>().CreateVectorStore());

        return services;
    }
}