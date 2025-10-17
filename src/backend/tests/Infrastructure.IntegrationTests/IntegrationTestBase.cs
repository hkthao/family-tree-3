using Testcontainers.MySql;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Data;

namespace backend.Infrastructure.IntegrationTests;

public class IntegrationTestFixture : IAsyncLifetime
{
    private MySqlContainer _mySqlContainer = null!;
    public ApplicationDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _mySqlContainer = new MySqlBuilder().Build();
        await _mySqlContainer.StartAsync();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(_mySqlContainer.GetConnectionString(), ServerVersion.AutoDetect(_mySqlContainer.GetConnectionString()));

        DbContext = new ApplicationDbContext(optionsBuilder.Options);
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _mySqlContainer.DisposeAsync();
    }

    public async Task ResetState()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.MigrateAsync();
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