using NUnit.Framework;
using Testcontainers.MongoDB;
using backend.Infrastructure.Data;
using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace backend.Infrastructure.IntegrationTests;

[SetUpFixture]
public class IntegrationTestBase
{
    private static MongoDBContainer _mongoDbContainer = null!;
    private static ApplicationDbContext _dbContext = null!;

    public static IApplicationDbContext GetDbContext()
    {
        return _dbContext;
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _mongoDbContainer = new MongoDBBuilder().Build();
        await _mongoDbContainer.StartAsync();

        var mongoDbSettings = Options.Create(new MongoDbSettings
        {
            ConnectionString = _mongoDbContainer.GetConnectionString(),
            DatabaseName = "testdb"
        });

        _dbContext = new ApplicationDbContext(mongoDbSettings);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _mongoDbContainer.DisposeAsync();
    }
}
