using backend.Application.Common.Interfaces;
using backend.Application.VectorStore;
using backend.Infrastructure.Auth;
using backend.Infrastructure.Data;
using backend.Infrastructure.Services;
using backend.Infrastructure.VectorStore;
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

        // Register Vector Store
        services.Configure<VectorStoreSettings>(configuration.GetSection("VectorStore"));
        services.AddSingleton<IVectorStoreSettings, VectorStoreSettings>();
        services.AddTransient<PineconeVectorStore>();

        services.AddSingleton<IVectorStoreFactory, VectorStoreFactory>();
        services.AddTransient<IVectorStore>(sp => sp.GetRequiredService<IVectorStoreFactory>().CreateVectorStore());

        return services;
    }
}
