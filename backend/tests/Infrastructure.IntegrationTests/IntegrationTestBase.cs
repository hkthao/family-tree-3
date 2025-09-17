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

        DbContext = new ApplicationDbContext(mongoDbSettings);
        await Task.CompletedTask; // Simulate async operation
    }

    public async Task DisposeAsync()
    {
        // await _mongoDbContainer.DisposeAsync(); // Commented out
        await Task.CompletedTask; // Simulate async operation
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

    protected IntegrationTestBase(IntegrationTestFixture fixture)
    {
        _dbContext = fixture.DbContext;
    }
}
