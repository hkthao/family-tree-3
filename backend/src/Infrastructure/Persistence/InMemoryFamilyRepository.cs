using backend.Domain.Entities;
using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Persistence;

public class InMemoryFamilyRepository : InMemoryRepository<Family>, IFamilyRepository
{
    private readonly IMemberRepository _memberRepository;

    public InMemoryFamilyRepository(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
        _items.AddRange(new List<Family>
        {
            new Family { Id = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"), Name = "Royal Family", Description = "A prominent family.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Public", TotalMembers = 20 },
            new Family { Id = Guid.Parse("3cfc98c1-cd0a-4d4d-bb6a-bdbc47dd27dc"), Name = "Tran Family", Description = "A historical lineage.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Private" },
            new Family { Id = Guid.Parse("548ce66d-2d39-4f8f-9326-959ffabb1632"), Name = "Le Family", Description = "Known for their resilience.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Public" },
            new Family { Id = Guid.Parse("58cc481c-8eb6-4664-be90-944e7f2453ae"), Name = "Pham Family", Description = "A family of scholars.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Private" },
            new Family { Id = Guid.Parse("76f11bc8-bb1b-4279-b610-db788325faea"), Name = "Huynh Family", Description = "Guardians of tradition.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Public" },
            new Family { Id = Guid.Parse("e5251d63-93c1-47e4-98b8-874bb8578f58"), Name = "Phan Family", Description = "Pioneers of innovation.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Private" },
            new Family { Id = Guid.Parse("6bd3a0fb-75f1-497e-8d08-f94ee4c136c3"), Name = "Vo Family", Description = "Strong and united.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Public" },
            new Family { Id = Guid.Parse("1b4a99e7-cd97-4532-9f7b-19473f94e563"), Name = "Dang Family", Description = "Rich in heritage.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Private" },
            new Family { Id = Guid.Parse("aff8c469-84aa-4d7a-98ac-c65aa145bac5"), Name = "Bui Family", Description = "Artistic and creative.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Public" },
            new Family { Id = Guid.Parse("dca44ea6-ff7b-44df-82fc-b20310b17594"), Name = "Do Family", Description = "Diligent and prosperous.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Private" },
            new Family { Id = Guid.Parse("15343afc-96c6-4515-a253-99800dd0cbd4"), Name = "Ngo Family", Description = "Respected and wise.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Public" },
            new Family { Id = Guid.Parse("65547dbf-fcc6-4a61-909d-c944998c4b72"), Name = "Duong Family", Description = "Generous and kind.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Private" },
            new Family { Id = Guid.Parse("c0658b43-c4a9-46d2-9c6d-3eff9cd9b10c"), Name = "Dinh Family", Description = "Brave and honorable.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Public" },
            new Family { Id = Guid.Parse("3dd26aff-0a34-497b-9178-d921378cc57e"), Name = "Hoang Family", Description = "Loyal and true.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Private" },
            new Family { Id = Guid.Parse("c71894b3-afa9-4311-bd50-d533905dcc14"), Name = "Truong Family", Description = "Strong and enduring.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Public" },
            new Family { Id = Guid.Parse("0a9d163e-2e01-4dd5-afc8-92185841860f"), Name = "Cao Family", Description = "Noble and proud.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Private" },
            new Family { Id = Guid.Parse("de9618b0-4be8-4cb6-8638-6a78d94c7e72"), Name = "Chau Family", Description = "Peaceful and serene.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Public" },
            new Family { Id = Guid.Parse("55b47990-9a1a-4918-8352-c4f42ef19492"), Name = "Bach Family", Description = "Pure and honest.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Private" },
            new Family { Id = Guid.Parse("06e3e46a-f2e4-42a3-9a36-b6466c690323"), Name = "Ly Family", Description = "Wise and influential.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Public" },
            new Family { Id = Guid.Parse("6c09f340-ac53-406b-9922-70a55b93a20b"), Name = "Vu Family", Description = "Powerful and respected.", AvatarUrl = "https://picsum.photos/200/300", Visibility = "Private" },
        });
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