using backend.Application.Common.Interfaces;
using backend.Application.VectorStore;
using backend.Infrastructure.Auth;
using backend.Infrastructure.Data;
using backend.Infrastructure.Services;
using backend.Infrastructure.VectorStore;
using backend.Infrastructure.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.Configure<ChatSettings>(configuration.GetSection("Chat"));
        services.AddTransient<ILLMProvider, GeminiProvider>();
        services.AddTransient<ILLMProvider, OpenAIProvider>();
        services.AddTransient<ILLMProvider, LocalAIProvider>();
        services.AddSingleton<ILLMProviderFactory, LLMProviderFactory>();
        services.AddScoped<IChatService, backend.Application.Chat.ChatService>();
        services.AddScoped<IEmbeddingGenerator, backend.Infrastructure.AI.EmbeddingGenerator>();

        // Register Vector Store
        services.Configure<VectorStoreSettings>(configuration.GetSection("VectorStore"));
        services.AddSingleton<IVectorStoreSettings, VectorStoreSettings>();
        services.AddTransient<PineconeVectorStore>();

        services.AddSingleton<IVectorStoreFactory, VectorStoreFactory>();
        services.AddTransient<IVectorStore>(sp => sp.GetRequiredService<IVectorStoreFactory>().CreateVectorStore());

        return services;
    }
}
