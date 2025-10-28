using System.Collections.Concurrent;
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
using backend.Domain.Events;
using MediatR;

namespace backend.CompositionRoot;

/// <summary>
/// Lớp mở rộng để đăng ký các dịch vụ gốc (Composition Root) vào bộ chứa dependency injection.
/// </summary>
public static class DependencyInjection
{

    /// <summary>
    /// Đăng ký tất cả các dịch vụ cần thiết cho ứng dụng, bao gồm các dịch vụ từ tầng Application và Infrastructure,
    /// cấu hình lưu trữ tệp và xác thực Auth0.
    /// </summary>
    /// <param name="services">Bộ sưu tập dịch vụ để đăng ký.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>Bộ sưu tập dịch vụ đã được cập nhật.</returns>
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
                    OnTokenValidated = async (context) =>
                    {
                        var externalId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        using var scope = context.HttpContext.RequestServices.CreateScope();
                        var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        try
                        {
                            var userLoggedInEvent = new UserLoggedInEvent(context.Principal!); // Create the event
                            await mediator.Publish(userLoggedInEvent); // Publish the event
                        }
                        catch (Exception ex)
                        {
                            scopedLogger.LogError(ex, "Error publishing UserLoggedInEvent for external ID: {ExternalId}. Details: {Error}", externalId, ex.Message);
                        }
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