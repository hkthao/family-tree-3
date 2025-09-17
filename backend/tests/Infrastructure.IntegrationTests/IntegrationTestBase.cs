using Xunit;
// using Testcontainers.MongoDb; // Commented out
using backend.Infrastructure.Data;
using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Threading.Tasks;
using System;

namespace backend.Infrastructure.IntegrationTests;

public class IntegrationTestFixture : IAsyncLifetime
{
    // private MongoDBContainer _mongoDbContainer = null!; // Commented out
    public ApplicationDbContext DbContext { get; private set; } = null!;
    private IMongoClient _mongoClient = null!;

    public async Task InitializeAsync()
    {
        // _mongoDbContainer = new MongoDBBuilder().Build(); // Commented out
        // await _mongoDbContainer.StartAsync(); // Commented out

        // Dummy MongoDB settings for compilation
        var mongoDbSettings = Options.Create(new AppMongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017", // Dummy connection string
            DatabaseName = "testdb"
        });

        _mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
        DbContext = new ApplicationDbContext(mongoDbSettings);
        await Task.CompletedTask; // Simulate async operation
    }

    public async Task DisposeAsync()
    {
        // await _mongoDbContainer.DisposeAsync(); // Commented out
        await Task.CompletedTask; // Simulate async operation
    }

    public async Task ResetState()
    {
        var mongoDbSettings = Options.Create(new AppMongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017", // Dummy connection string
            DatabaseName = "testdb"
        });
        await _mongoClient.DropDatabaseAsync(mongoDbSettings.Value.DatabaseName);
    }
}

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture interfaces.
}

public abstract class IntegrationTestBase : IClassFixture<IntegrationTestFixture>
{
    protected readonly ApplicationDbContext _dbContext;
    private readonly IntegrationTestFixture _fixture;

    protected IntegrationTestBase(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _dbContext = fixture.DbContext;
        _fixture.ResetState().GetAwaiter().GetResult(); // Reset state before each test
    }
}