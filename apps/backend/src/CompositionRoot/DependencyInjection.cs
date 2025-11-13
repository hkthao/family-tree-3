using backend.Application;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Infrastructure;
using backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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
