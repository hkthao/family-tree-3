
using backend.Application.Common.Interfaces;
using backend.Web.Services;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;
using Microsoft.AspNetCore.Authentication;
using backend.Infrastructure.Auth;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();


        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument((configure, sp) =>
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

        builder.Services.AddTransient<IClaimsTransformation, Auth0ClaimsTransformer>();

        // Add Authentication and Authorization
        // builder.Services.AddAuthentication(options =>
        // {
        //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        // })
        //     .AddJwtBearer(options =>
        //     {
        //         options.Authority = builder.Configuration["Auth0:Domain"];
        //         options.Audience = builder.Configuration["Auth0:Audience"];
        //         options.RequireHttpsMetadata = false;
        //     });

        // builder.Services.AddAuthorizationBuilder();
    }

}
