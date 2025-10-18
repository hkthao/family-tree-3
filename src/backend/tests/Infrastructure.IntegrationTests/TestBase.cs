using backend.Domain.Entities; // Add this using directive
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.IntegrationTests;

public abstract class TestBase : IDisposable
{
    protected ApplicationDbContext Context { get; }
    protected Family DefaultFamily { get; } // Add DefaultFamily property

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"DataSource=file:{Guid.NewGuid()}.db?mode=memory&cache=shared")
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.OpenConnection();
        Context.Database.EnsureCreated();

        // Seed default family
        DefaultFamily = new Family { Name = "Default Family" };
        Context.Families.Add(DefaultFamily);
        Context.SaveChanges(); // Use SaveChanges() for synchronous seeding

        // Seed data here if needed
        // Example: SeedData(Context);
    }

    public void Dispose()
    {
        Context.Database.CloseConnection();
        Context.Dispose();
    }

    // Optional: Method to seed data
    protected virtual void SeedData(ApplicationDbContext context)
    {
        // Implement seeding logic in derived classes or here
    }

    // Helper method to create a Member with default family
    protected Member CreateMember(string firstName, string lastName, string gender, DateTime dateOfBirth)
    {
        return new Member
        {
            FirstName = firstName,
            LastName = lastName,
            Gender = gender,
            DateOfBirth = dateOfBirth,
            FamilyId = DefaultFamily.Id // Assign to default family
        };
    }
}
