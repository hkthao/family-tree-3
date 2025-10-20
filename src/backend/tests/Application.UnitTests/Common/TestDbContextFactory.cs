using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using backend.Application.UnitTests.Common;

namespace backend.Application.UnitTests.Common;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(bool seedData = false)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        if (seedData)
            SeedSampleData(context);
        return context;
    }

    public static void Destroy(ApplicationDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }

    private static void SeedSampleData(ApplicationDbContext context)
    {
        SampleDataSeeder.SeedSampleData(context);
    }
}
