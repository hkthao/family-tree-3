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
        // Configure AppMongoDbSettings
        services.Configure<AppMongoDbSettings>(configuration.GetSection("MongoDbSettings"));

        // Register IMongoClient and IMongoDatabase
        services.AddSingleton<IMongoClient>(sp => new MongoClient(sp.GetRequiredService<IOptions<AppMongoDbSettings>>().Value.ConnectionString));
        services.AddScoped<IMongoDatabase>(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(sp.GetRequiredService<IOptions<AppMongoDbSettings>>().Value.DatabaseName));

        // Register IApplicationDbContext and ApplicationDbContext
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        services.AddIdentity<ApplicationUser, MongoIdentityRole<ObjectId>>()
            .AddMongoDbStores<ApplicationUser, MongoIdentityRole<ObjectId>, ObjectId>(
                configuration.GetSection("MongoDbSettings:ConnectionString").Value,
                configuration.GetSection("MongoDbSettings:DatabaseName").Value
            )
            .AddDefaultTokenProviders();

        services.AddSingleton(TimeProvider.System);
        services.AddTransient<IIdentityService, IdentityService>();

        services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        return services;
    }
}
