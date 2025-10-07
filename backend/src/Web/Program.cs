using System.Security.Claims;
using backend.Application;
using backend.Infrastructure;
using backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using backend.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using backend.Application.Common.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure Auth0Config
builder.Services.Configure<Auth0Config>(builder.Configuration.GetSection("Auth0"));
builder.Services.AddSingleton<IAuth0Config>(sp => sp.GetRequiredService<IOptions<Auth0Config>>().Value);

// Read Auth0 configuration from environment variables
var auth0Domain = builder.Configuration["Auth0:Domain"];
var auth0Audience = builder.Configuration["Auth0:Audience"];
var auth0Namespace = builder.Configuration["Auth0:Namespace"];

// Log Auth0 configuration for debugging
builder.Logging.AddConsole(); // Ensure console logging is enabled
var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Auth0Config");
logger.LogInformation("Auth0 Domain: {Auth0Domain}", auth0Domain);
logger.LogInformation("Auth0 Audience: {Auth0Audience}", auth0Audience);
logger.LogInformation("Auth0 Namespace: {Auth0Namespace}", auth0Namespace);

// Configure Auth0 Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = auth0Domain;
        options.Audience = auth0Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = auth0Audience, // Explicitly set the valid audience
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var userProfileSyncService = context.HttpContext.RequestServices.GetRequiredService<IUserProfileSyncService>();
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                // Run the sync operation in a background task to not block the main request thread
                        _ = Task.Run(async () =>
                        {
                            using (var scope = context.HttpContext.RequestServices.CreateScope())
                            {
                                var scopedUserProfileSyncService = scope.ServiceProvider.GetRequiredService<backend.Application.Common.Interfaces.IUserProfileSyncService>();
                                var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                                try
                                {
                                    await scopedUserProfileSyncService.SyncUserProfileAsync(context.Principal!);

                                    // Record Login Activity
                                    var userProfileId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                                    if (!string.IsNullOrEmpty(userProfileId))
                                    {
                                        var recordCommand = new backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand
                                        {
                                            UserProfileId = Guid.Parse(userProfileId),
                                            ActionType = backend.Domain.Enums.UserActionType.Login,
                                            TargetType = backend.Domain.Enums.TargetType.UserProfile,
                                            TargetId = Guid.Parse(userProfileId),
                                            ActivitySummary = "User logged in."
                                        };
                                        await mediator.Send(recordCommand);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    scopedLogger.LogError(ex, "Error syncing user profile or recording login activity for Auth0 user {Auth0UserId}.", context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                                }
                            }
                        });
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Example policies, adjust as needed
    options.AddPolicy("read:messages", policy => policy.RequireClaim("permissions", "read:messages"));
    options.AddPolicy("write:messages", policy => policy.RequireClaim("permissions", "write:messages"));
});

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173", "https://localhost:5173", "https://localhost:5001") // Frontend development server URL
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.AddWebServices();

// Add controllers service
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Initialise and seed database
    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Use CORS policy
app.UseCors("AllowFrontend");

app.UseHealthChecks("/health");
// app.UseHttpsRedirection(); // Disabled to allow HTTP access for local development
app.UseStaticFiles();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});

app.UseAuthentication(); // Add Authentication middleware
app.UseAuthorization();  // Add Authorization middleware

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/api"));

// app.MapEndpoints(); // Removed this line

// Map controllers
app.MapControllers();

app.Run();

public partial class Program { }
