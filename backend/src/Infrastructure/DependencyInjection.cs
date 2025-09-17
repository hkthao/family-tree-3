using backend.Application.Common.Interfaces;
using backend.Domain.Constants;
using backend.Infrastructure.Data;
using backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using AspNetCore.Identity.MongoDbCore.Models;

namespace backend.Infrastructure;

public static class DependencyInjection
{
public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
{
    // Configure AppMongoDbSettings from appsettings.json or environment variables
    services.Configure<AppMongoDbSettings>(configuration.GetSection("MongoDbSettings"));

    // Check if we are running in an NSwag build context
    var isNSwagBuild = Environment.GetEnvironmentVariable("NSWAG_BUILD") == "true";

    if (isNSwagBuild)
    {
        // Override with fake values for NSwag build
        services.PostConfigure<AppMongoDbSettings>(settings =>
        {
            settings.ConnectionString = "mongodb://localhost:27017"; // Fake connection string
            settings.DatabaseName = "dummy-db"; // Fake database name
        });
    }

    // Register IMongoClient and IMongoDatabase
    services.AddSingleton<IMongoClient>(sp => new MongoClient(sp.GetRequiredService<IOptions<AppMongoDbSettings>>().Value.ConnectionString));
    services.AddScoped<IMongoDatabase>(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(sp.GetRequiredService<IOptions<AppMongoDbSettings>>().Value.DatabaseName));

    // Register IApplicationDbContext and ApplicationDbContext
    services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

    // Add MongoDB Identity stores
    services.AddIdentity<ApplicationUser, MongoIdentityRole<ObjectId>>()
        .AddMongoDbStores<ApplicationUser, MongoIdentityRole<ObjectId>, ObjectId>(
            // Retrieve connection string and database name from configuration
            configuration.GetSection("MongoDbSettings:ConnectionString").Value,
            configuration.GetSection("MongoDbSettings:DatabaseName").Value
        )
        .AddDefaultTokenProviders();

    // --- Common Services (always registered) ---
    services.AddSingleton(TimeProvider.System);
    services.AddTransient<IIdentityService, IdentityService>();

    services.AddAuthorization(options =>
        options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

    return services;
}
