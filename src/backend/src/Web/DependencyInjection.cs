
using backend.Application.Common.Interfaces;
using backend.Infrastructure.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IServiceCollection services)
    {
        services.AddHealthChecks();
        services.AddExceptionHandler<CustomExceptionHandler>();
        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);
        services.AddEndpointsApiExplorer();
        services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "API Cây Gia Phả";
            configure.Description = "API để quản lý thông tin cây gia phả, bao gồm các dòng họ, thành viên, và các mối quan hệ.";

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
    }
}
