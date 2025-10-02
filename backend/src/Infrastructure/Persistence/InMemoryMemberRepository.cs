using backend.Domain.Entities;
using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Persistence;

public class InMemoryMemberRepository : InMemoryRepository<Member>, IMemberRepository
{
    public InMemoryMemberRepository()
    {
        // Seed some initial data for testing
        var member1 = new Member { Id = Guid.Parse("c3d4e5f6-a7b8-9012-3456-7890abcdef12"), FullName = "Nguyen Van A", FamilyId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"), Gender = "Male", Generation = 1 };
        var member2 = new Member { Id = Guid.Parse("d4e5f6a7-b8c9-0123-4567-890abcdef345"), FullName = "Tran Thi B", FamilyId = Guid.Parse("b2c3d4e5-f6a7-8901-2345-67890abcdef0"), Gender = "Female", Generation = 2 };
        var member3 = new Member { Id = Guid.NewGuid(), FullName = "Nguyen Van C", FamilyId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"), Gender = "Male", Generation = 2, FatherId = member1.Id, MotherId = member2.Id };

        _items.Add(member1);
        _items.Add(member2);
        _items.Add(member3);
    }

    public override async Task<Member?> GetByIdAsync(Guid id)
    {
        var member = await base.GetByIdAsync(id);
        if (member != null)
        {
            await PopulateRelationships(member);
        }
        return member;
    }

    public override async Task<IReadOnlyList<Member>> GetAllAsync()
    {
        var members = await base.GetAllAsync();
        foreach (var member in members)
        {
            await PopulateRelationships(member);
        }
        return members;
    }

    private async Task PopulateRelationships(Member member)
    {
        if (member.FatherId.HasValue)
        {
            member.Father = await base.GetByIdAsync(member.FatherId.Value);
        }
        if (member.MotherId.HasValue)
        {
            member.Mother = await base.GetByIdAsync(member.MotherId.Value);
        }
        if (member.SpouseId.HasValue)
        {
            member.Spouse = await base.GetByIdAsync(member.SpouseId.Value);
        }
        member.Children = _items.Where(m => m.FatherId == member.Id || m.MotherId == member.Id).ToList();
    }
}