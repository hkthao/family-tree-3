using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace backend.Infrastructure.IntegrationTests;

public class IntegrationTestFixture : IAsyncLifetime
{
    public ApplicationDbContext DbContext { get; private set; } = null!;

    public Task InitializeAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()); // Use in-memory database

        DbContext = new ApplicationDbContext(optionsBuilder.Options);
        DbContext.Database.EnsureCreated(); // Ensure database is created

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync(); // Ensure database is deleted
        await DbContext.DisposeAsync();
    }

    public async Task ResetState()
    {
        await DbContext.Database.EnsureDeletedAsync();
        DbContext.Database.EnsureCreated();
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