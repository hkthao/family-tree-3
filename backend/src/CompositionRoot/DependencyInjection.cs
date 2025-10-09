using backend.Application;
using backend.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using backend.Application.Common.Models;
using backend.Application.Common.Interfaces;
using backend.Infrastructure.AI;
using backend.Domain.Enums;
using Microsoft.Extensions.Options;
using backend.Infrastructure.Files;
using Microsoft.AspNetCore.Hosting;
using backend.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using backend.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace backend.CompositionRoot;

public static class DependencyInjection
{
    public static IServiceCollection AddCompositionRootServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);

        // Configure AIConfig
        services.Configure<AIConfig>(configuration.GetSection("AI"));
        services.AddSingleton<IAISettings>(sp => sp.GetRequiredService<IOptions<AIConfig>>().Value);

        // Register AI Content Generators
        services.AddTransient<GeminiAIContentGenerator>();
        services.AddTransient<OpenAIAIContentGenerator>();
        services.AddTransient<LocalAIContentGenerator>();

        services.AddTransient<IAIContentGenerator>(sp =>
        {
            var aiConfig = sp.GetRequiredService<IOptions<AIConfig>>().Value;
            return aiConfig.Provider switch
            {
                AIProviderType.Gemini => sp.GetRequiredService<GeminiAIContentGenerator>(),
                AIProviderType.OpenAI => sp.GetRequiredService<OpenAIAIContentGenerator>(),
                AIProviderType.LocalAI => sp.GetRequiredService<LocalAIContentGenerator>(),
                _ => throw new InvalidOperationException($"No AI content generator configured for provider: {aiConfig.Provider}")
            };
        });

        // Register AI Usage Tracker
        services.AddSingleton<IAIUsageTracker, AIUsageTracker>();

        // Add Memory Cache services
        services.AddMemoryCache();

        // Configure StorageSettings
        services.Configure<StorageSettings>(configuration.GetSection("Storage"));
        services.AddSingleton<IStorageSettings>(sp => sp.GetRequiredService<IOptions<StorageSettings>>().Value);

        // Register IFileStorageService based on configuration
        services.AddTransient<IFileStorageService>(sp =>
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
                options.Authority = configuration["Auth0:Domain"];
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
