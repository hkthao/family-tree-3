using System.Security.Claims;
using backend.Application;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;
using backend.Domain.Enums;
using backend.Infrastructure;

using backend.Infrastructure.Data;
using backend.Infrastructure.Files;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace backend.CompositionRoot;

public static class DependencyInjection
{
    public static IServiceCollection AddCompositionRootServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationServices(configuration);
        services.AddInfrastructureServices(configuration);



        services.AddScoped<IFileStorageFactory, FileStorageFactory>();
        // Register IFileStorage based on configuration
        services.AddTransient(sp =>
        {
            var factory = sp.GetRequiredService<IFileStorageFactory>();
            var configProvider = sp.GetRequiredService<IConfigProvider>();
            var storageSettings = configProvider.GetSection<StorageSettings>();
            return factory.CreateFileStorage(Enum.Parse<StorageProvider>(storageSettings.Provider, true));
        });


        var configProvider = services.BuildServiceProvider().GetRequiredService<IConfigProvider>();
        var jwtSettings = configProvider.GetSection<JwtSettings>();

        // Configure Auth0 Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {

                // Add logging for JwtSettings
                var logger = services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
                logger.LogInformation("JwtSettings - Authority: {Authority}", jwtSettings?.Authority);
                logger.LogInformation("JwtSettings - Audience: {Audience}", jwtSettings?.Audience);
                logger.LogInformation("JwtSettings - Namespace: {Namespace}", jwtSettings?.Namespace);

                options.Authority = $"https://{jwtSettings?.Authority}";
                options.Audience = jwtSettings?.Audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://{jwtSettings?.Authority}",
                    ValidateAudience = true,
                    ValidAudience = jwtSettings?.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                        _ = Task.Run(async () =>
                        {
                            using var scope = context.HttpContext.RequestServices.CreateScope();
                            var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                            var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

                            try
                            {
                                var command = new SyncUserProfileCommand
                                {
                                    UserPrincipal = context.Principal!
                                };
                                var result = await mediator.Send(command);
                                if (!result.IsSuccess)
                                {
                                    scopedLogger.LogError("Error syncing user profile for external ID: {ExternalId}. Details: {Error}", context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value, result.Error);
                                }
                            }
                            catch (Exception ex)
                            {
                                scopedLogger.LogError(ex, "Error syncing user profile for external ID: {ExternalId}. Details: {Error}", context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value, ex.Message);
                            }
                        });
                        return Task.CompletedTask;
                    },

                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("read:messages", policy => policy.RequireClaim("permissions", "read:messages"));
            options.AddPolicy("write:messages", policy => policy.RequireClaim("permissions", "write:messages"));
        });

        services.AddScoped<backend.Domain.Services.IChunkingPolicy, backend.Domain.Services.ChunkingPolicy>();

        services.AddScoped<ApplicationDbContextInitialiser>();

        return services;
    }
}
public partial class Program { }
