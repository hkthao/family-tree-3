using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Common.Configurations; // Added for VoiceAISettings
using backend.Infrastructure.Auth;
using backend.Infrastructure.Services;
using backend.Web.Filters; // Add this using directive for the filter
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks();
        services.AddExceptionHandler<CustomExceptionHandler>();
        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = false); // Re-enable ModelStateInvalidFilter

        services.AddEndpointsApiExplorer();
        services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "API Dòng Họ Việt";
            configure.Description = "API để quản lý thông tin Dòng Họ Việt, bao gồm các dòng họ, thành viên, và các mối quan hệ.";

            // Add JWT
            configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Nhập JWT token theo định dạng: Bearer {your JWT token}"
            });

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });
        services.AddTransient<IClaimsTransformation, Auth0ClaimsTransformer>();

        // Register BotDetection settings
        services.Configure<BotDetectionSettings>(options =>
        {
            // Set default values if not configured in appsettings.json
            options.BlacklistedUserAgents = new[] { "bot", "crawl", "spider", "scraper", "http client", "python-requests", "postmanruntime", "curl", "java", "axios", "go-http-client" };
            // BlockEmptyUserAgent is now ignored for blocking, only for logging.
            options.AllowedAcceptHeaders = new[] { "application/json", "*/*" };
        });

        // Register API Key settings - will be loaded from appsettings.json
        services.Configure<ApiKeySettings>(configuration.GetSection(nameof(ApiKeySettings)));

        // Register the BotDetectionActionFilter
        services.AddScoped<BotDetectionActionFilter>();

        // Register the ApiKeyAuthenticationFilter
        services.AddScoped<ApiKeyAuthenticationFilter>();

        // Register OcrSettings
        services.Configure<OcrSettings>(configuration.GetSection(nameof(OcrSettings)));
        services.AddHttpClient<IOcrService, OcrService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetSection(nameof(OcrSettings))["BaseUrl"] ?? throw new InvalidOperationException("OCR BaseUrl is not configured."));
        });
        
        // Register VoiceAISettings
        services.Configure<VoiceAISettings>(configuration.GetSection(nameof(VoiceAISettings)));
        services.AddHttpClient<IVoiceAIService, VoiceAIService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetSection(nameof(VoiceAISettings))["BaseUrl"] ?? throw new InvalidOperationException("Voice AI BaseUrl is not configured."));
        });
    }
}
