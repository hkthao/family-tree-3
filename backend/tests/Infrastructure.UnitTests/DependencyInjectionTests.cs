using backend.Application.Common.Interfaces;
using backend.Infrastructure.Data;
using backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using AspNetCore.Identity.MongoDbCore.Models;

namespace backend.Infrastructure.UnitTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructureServices_ShouldAddRequiredServices_WhenNotNSwagBuild()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"MongoDbSettings:ConnectionString", "mongodb://localhost:27017"},
                {"MongoDbSettings:DatabaseName", "testdb"}
            })
            .Build();

        Environment.SetEnvironmentVariable("NSWAG_BUILD", "false");

        // Act
        services.AddInfrastructureServices(configuration);

        // Assert
        services.Should().NotBeNull();
        services.Should().ContainSingle(s => s.ServiceType == typeof(IMongoClient));
        services.Should().ContainSingle(s => s.ServiceType == typeof(IMongoDatabase));
        services.Should().ContainSingle(s => s.ServiceType == typeof(IApplicationDbContext));
        services.Should().ContainSingle(s => s.ServiceType == typeof(IIdentityService));
        services.Should().ContainSingle(s => s.ServiceType == typeof(TimeProvider));

        // Verify Identity services
        services.Should().ContainSingle(s => s.ServiceType == typeof(UserManager<ApplicationUser>));
        services.Should().ContainSingle(s => s.ServiceType == typeof(SignInManager<ApplicationUser>));
        services.Should().ContainSingle(s => s.ServiceType == typeof(RoleManager<MongoIdentityRole<ObjectId>>));

        // Clean up environment variable
        Environment.SetEnvironmentVariable("NSWAG_BUILD", null);
    }

    [Fact]
    public void AddInfrastructureServices_ShouldAddDummyServices_WhenNSwagBuild()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build(); // Configuration might be empty in NSwag build

        Environment.SetEnvironmentVariable("NSWAG_BUILD", "true");

        // Act
        services.AddInfrastructureServices(configuration);

        // Assert
        services.Should().NotBeNull();
        services.Should().ContainSingle(s => s.ServiceType == typeof(IMongoClient));
        services.Should().ContainSingle(s => s.ServiceType == typeof(IMongoDatabase));
        services.Should().ContainSingle(s => s.ServiceType == typeof(IApplicationDbContext));
        services.Should().ContainSingle(s => s.ServiceType == typeof(IIdentityService));
        services.Should().ContainSingle(s => s.ServiceType == typeof(TimeProvider));

        // Verify Identity services
        services.Should().ContainSingle(s => s.ServiceType == typeof(UserManager<ApplicationUser>));
        services.Should().ContainSingle(s => s.ServiceType == typeof(SignInManager<ApplicationUser>));
        services.Should().ContainSingle(s => s.ServiceType == typeof(RoleManager<MongoIdentityRole<ObjectId>>));

        // Clean up environment variable
        Environment.SetEnvironmentVariable("NSWAG_BUILD", null);
    }
}
