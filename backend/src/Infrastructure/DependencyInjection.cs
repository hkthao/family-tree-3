using backend.Application.Common.Interfaces;
using backend.Infrastructure.Data;
using backend.Infrastructure.Services;
using backend.Infrastructure.AI.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using backend.Application.AI.VectorStore;
using backend.Infrastructure.AI.Embeddings;
using backend.Application.AI.Chat;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AISettings;
using Microsoft.Extensions.Options;
using backend.Infrastructure.AI.VectorStore;

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
        services.AddTransient<OpenAIEmbeddingProvider>();
        services.AddTransient<CohereEmbeddingProvider>();
        services.AddTransient<LocalEmbeddingProvider>();
        services.AddScoped<IEmbeddingProviderFactory, EmbeddingProviderFactory>();
        // Register Vector Store
        services.Configure<VectorStoreSettings>(configuration.GetSection(VectorStoreSettings.SectionName));
        services.AddTransient<InMemoryVectorStore>();
        services.AddTransient<PineconeVectorStore>();
        services.AddScoped<IVectorStoreFactory, VectorStoreFactory>();

        services.AddScoped<IFileTextExtractorFactory, FileTextExtractorFactory>();

        services.AddTransient<PdfTextExtractor>();
        services.AddTransient<TxtTextExtractor>();

        return services;
    }
}