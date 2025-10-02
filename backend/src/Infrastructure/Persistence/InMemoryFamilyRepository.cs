using backend.Domain.Entities;
using backend.Application.Common.Interfaces;
using System.Linq;

namespace backend.Infrastructure.Persistence;

public class InMemoryFamilyRepository : InMemoryRepository<Family>, IFamilyRepository
{
    private readonly IMemberRepository _memberRepository;

    public InMemoryFamilyRepository(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
        // Seed some initial data for testing
        _items.Add(new Family { Id = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"), Name = "Nguyen Family", Description = "A prominent family in Vietnam", Visibility = "Public", AvatarUrl = "https://picsum.photos/200/300" });
        _items.Add(new Family { Id = Guid.Parse("b2c3d4e5-f6a7-8901-2345-67890abcdef0"), Name = "Tran Family", Description = "A historical family", Visibility = "Private", AvatarUrl = "https://picsum.photos/200/300" });
        _items.Add(new Family { Id = Guid.NewGuid(), Name = "Le Family", Description = "A shared family", Visibility = "Shared", AvatarUrl = "https://picsum.photos/200/300" });
    }

    public override async Task<Family?> GetByIdAsync(Guid id)
    {
        var family = await base.GetByIdAsync(id);
        if (family != null)
        {
            await PopulateTotalMembers(family);
        }
        return family;
    }

    public override async Task<IReadOnlyList<Family>> GetAllAsync()
    {
        var families = await base.GetAllAsync();
        foreach (var family in families)
        {
            await PopulateTotalMembers(family);
        }
        return families;
    }

    private async Task PopulateTotalMembers(Family family)
    {
        family.TotalMembers = await _memberRepository.CountMembersByFamilyIdAsync(family.Id);
    }
}