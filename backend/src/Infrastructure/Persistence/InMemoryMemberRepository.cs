using backend.Domain.Entities;
using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Persistence;

public class InMemoryMemberRepository : InMemoryRepository<Member>, IMemberRepository
{
    public InMemoryMemberRepository()
    {
        // Seed some initial data for testing
        _items.Add(new Member { Id = Guid.Parse("c3d4e5f6-a7b8-9012-3456-7890abcdef12"), FullName = "Nguyen Van A", FamilyId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"), Gender = "Male", Generation = 1 });
        _items.Add(new Member { Id = Guid.Parse("d4e5f6a7-b8c9-0123-4567-890abcdef345"), FullName = "Tran Thi B", FamilyId = Guid.Parse("b2c3d4e5-f6a7-8901-2345-67890abcdef0"), Gender = "Female", Generation = 2 });
    }
}