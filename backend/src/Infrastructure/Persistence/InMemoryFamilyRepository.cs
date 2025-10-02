using backend.Domain.Entities;
using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Persistence;

public class InMemoryFamilyRepository : InMemoryRepository<Family>, IFamilyRepository
{
    public InMemoryFamilyRepository()
    {
        // Seed some initial data for testing
        _items.Add(new Family { Id = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"), Name = "Nguyen Family", Description = "A prominent family in Vietnam", Visibility = "Public" });
        _items.Add(new Family { Id = Guid.Parse("b2c3d4e5-f6a7-8901-2345-67890abcdef0"), Name = "Tran Family", Description = "A historical family", Visibility = "Private" });
    }
}