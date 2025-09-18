using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.tests.Application.UnitTests.Common;

public abstract class TestBase
{
    protected ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated(); // Ensure the in-memory database is created

        return context;
    }
}
