using System.Security.Claims;
using backend.Application;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Infrastructure.AI;
using backend.Application.AI.ContentGenerators;
using backend.Infrastructure.Auth;
using backend.Infrastructure.Data;
using backend.Infrastructure.Files;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using backend.Infrastructure.AI.ContentGenerators;
using backend.Infrastructure;

namespace backend.CompositionRoot;

public static class DependencyInjection
{
    public static IServiceCollection AddCompositionRootServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationServices(configuration);
        services.AddInfrastructureServices(configuration);

        // Register AI Content Generators
        services.AddTransient<GeminiAIContentGenerator>();
        services.AddTransient<OpenAIAIContentGenerator>();
        services.AddTransient<LocalAIContentGenerator>();
        services.AddScoped<IAIContentGeneratorFactory, AIContentGeneratorFactory>();
        services.AddScoped(sp => sp.GetRequiredService<IAIContentGeneratorFactory>().GetContentGenerator());

        // Register AI Usage Tracker
        services.AddSingleton<IAIUsageTracker, AIUsageTracker>();

        // Add Memory Cache services
        services.AddMemoryCache();

        // Configure StorageSettings
        services.Configure<StorageSettings>(configuration.GetSection("Storage"));
        services.AddSingleton<IStorageSettings>(sp => sp.GetRequiredService<IOptions<StorageSettings>>().Value);

        // Register IFileStorage based on configuration
        services.AddTransient<IFileStorage>(sp =>
        {
            var storageSettings = sp.GetRequiredService<IStorageSettings>();
            var env = sp.GetRequiredService<IWebHostEnvironment>();

            return storageSettings.Provider switch
            {
                "Local" => new LocalFileStorage(storageSettings, env),
                "Cloudinary" => new CloudinaryFileStorage(storageSettings),
                "S3" => new S3FileStorage(storageSettings),
                _ => throw new InvalidOperationException($"No file storage provider configured for: {storageSettings.Provider}")
            };
        });

        // Configure Auth0Config
        services.Configure<Auth0Config>(configuration.GetSection("Auth0"));
        services.AddSingleton<IAuth0Config>(sp => sp.GetRequiredService<IOptions<Auth0Config>>().Value);

        // Configure Auth0 Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://{configuration["Auth0:Domain"]}";
                options.Audience = configuration["Auth0:Audience"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configuration["Auth0:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var userProfileSyncService = context.HttpContext.RequestServices.GetRequiredService<IUserProfileSyncService>();
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                        _ = Task.Run(async () =>
                        {
                            using (var scope = context.HttpContext.RequestServices.CreateScope())
                            {
                                var scopedUserProfileSyncService = scope.ServiceProvider.GetRequiredService<IUserProfileSyncService>();
                                var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                                var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

                                try
                                {
                                    var newUserCreated = await scopedUserProfileSyncService.SyncUserProfileAsync(context.Principal!);
                                }
                                catch (Exception ex)
                                {
                                    scopedLogger.LogError(ex, "Error syncing user profile for external ID: {ExternalId}.", context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                                }
                            }
                        });
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("read:messages", policy => policy.RequireClaim("permissions", "read:messages"));
            options.AddPolicy("write:messages", policy => policy.RequireClaim("permissions", "write:messages"));
        });

        services.AddScoped<ApplicationDbContextInitialiser>();

        return services;
    }
}
public partial class Program { }
