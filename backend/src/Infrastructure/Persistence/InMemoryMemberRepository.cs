using backend.Domain.Entities;
using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Persistence;

public class InMemoryMemberRepository : InMemoryRepository<Member>, IMemberRepository
{
    public InMemoryMemberRepository()
    {
        // Family ID for the British Royal Family (from frontend families.json)
        Guid royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb");

        // Define GUIDs for key members
        Guid elizabethIIId = Guid.NewGuid();
        Guid philipId = Guid.NewGuid();
        Guid charlesIIIId = Guid.NewGuid();
        Guid camillaId = Guid.NewGuid();
        Guid anneId = Guid.NewGuid();
        Guid andrewId = Guid.NewGuid();
        Guid edwardId = Guid.NewGuid();
        Guid williamId = Guid.NewGuid();
        Guid catherineId = Guid.NewGuid();
        Guid georgeId = Guid.NewGuid();
        Guid charlotteId = Guid.NewGuid();
        Guid louisId = Guid.NewGuid();
        Guid harryId = Guid.NewGuid();
        Guid meghanId = Guid.NewGuid();
        Guid archieId = Guid.NewGuid();
        Guid dianaId = Guid.NewGuid(); // Deceased, but for lineage
        Guid beatriceId = Guid.NewGuid();
        Guid eugenieId = Guid.NewGuid();
        Guid louiseId = Guid.NewGuid();
        Guid jamesId = Guid.NewGuid();
        Guid sarahId = Guid.NewGuid(); // Sarah Ferguson
        Guid timothyId = Guid.NewGuid(); // Timothy Laurence
        Guid sophieId = Guid.NewGuid(); // Sophie, Duchess of Edinburgh
        Guid lilibetId = Guid.NewGuid(); // Lilibet Mountbatten-Windsor

        _items.AddRange(new List<Member>
        {
            // Generation 0 (for context)
            new Member { Id = elizabethIIId, FullName = "Queen Elizabeth II", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1926, 4, 21), DateOfDeath = new DateTime(2022, 9, 8), SpouseId = philipId, AvatarUrl = "https://picsum.photos/200/300?random=1" },
            new Member { Id = philipId, FullName = "Prince Philip, Duke of Edinburgh", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1921, 6, 10), DateOfDeath = new DateTime(2021, 4, 9), SpouseId = elizabethIIId, AvatarUrl = "https://picsum.photos/200/300?random=2" },
            new Member { Id = dianaId, FullName = "Diana, Princess of Wales", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1961, 7, 1), DateOfDeath = new DateTime(1997, 8, 31), SpouseId = charlesIIIId, AvatarUrl = "https://picsum.photos/200/300?random=3" },
            new Member { Id = camillaId, FullName = "Camilla, Queen Consort", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1947, 7, 17), SpouseId = charlesIIIId, AvatarUrl = "https://picsum.photos/200/300?random=4" },
            new Member { Id = sarahId, FullName = "Sarah Ferguson", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1959, 10, 15), SpouseId = andrewId, AvatarUrl = "https://picsum.photos/200/300?random=5" },
            new Member { Id = timothyId, FullName = "Sir Timothy Laurence", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1955, 3, 1), SpouseId = anneId, AvatarUrl = "https://picsum.photos/200/300?random=6" },
            new Member { Id = sophieId, FullName = "Sophie, Duchess of Edinburgh", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1965, 1, 20), SpouseId = edwardId, AvatarUrl = "https://picsum.photos/200/300?random=7" },
            new Member { Id = catherineId, FullName = "Catherine, Princess of Wales", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1982, 1, 9), SpouseId = williamId, AvatarUrl = "https://picsum.photos/200/300?random=8" },
            new Member { Id = meghanId, FullName = "Meghan, Duchess of Sussex", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1981, 8, 4), SpouseId = harryId, AvatarUrl = "https://picsum.photos/200/300?random=9" },


            // Generation 1 (Children of Elizabeth II and Philip)
            new Member { Id = charlesIIIId, FullName = "King Charles III", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1948, 11, 14), FatherId = philipId, MotherId = elizabethIIId, SpouseId = camillaId, AvatarUrl = "https://picsum.photos/200/300?random=10" },
            new Member { Id = anneId, FullName = "Anne, Princess Royal", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1950, 8, 15), FatherId = philipId, MotherId = elizabethIIId, SpouseId = timothyId, AvatarUrl = "https://picsum.photos/200/300?random=11" },
            new Member { Id = andrewId, FullName = "Prince Andrew, Duke of York", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1960, 2, 19), FatherId = philipId, MotherId = elizabethIIId, SpouseId = sarahId, AvatarUrl = "https://picsum.photos/200/300?random=12" },
            new Member { Id = edwardId, FullName = "Prince Edward, Duke of Edinburgh", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1964, 3, 10), FatherId = philipId, MotherId = elizabethIIId, SpouseId = sophieId, AvatarUrl = "https://picsum.photos/200/300?random=13" },

            // Generation 2 (Children of Charles III and Diana) and Andrew/Sarah, Edward/Sophie
            new Member { Id = williamId, FullName = "Prince William, Prince of Wales", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1982, 6, 21), FatherId = charlesIIIId, MotherId = dianaId, SpouseId = catherineId, AvatarUrl = "https://picsum.photos/200/300?random=14" },
            new Member { Id = harryId, FullName = "Prince Harry, Duke of Sussex", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1984, 9, 15), FatherId = charlesIIIId, MotherId = dianaId, SpouseId = meghanId, AvatarUrl = "https://picsum.photos/200/300?random=15" },
            new Member { Id = beatriceId, FullName = "Princess Beatrice of York", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1988, 8, 8), FatherId = andrewId, MotherId = sarahId, AvatarUrl = "https://picsum.photos/200/300?random=16" },
            new Member { Id = eugenieId, FullName = "Princess Eugenie of York", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1990, 3, 23), FatherId = andrewId, MotherId = sarahId, AvatarUrl = "https://picsum.photos/200/300?random=17" },
            new Member { Id = louiseId, FullName = "Lady Louise Windsor", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(2003, 11, 8), FatherId = edwardId, MotherId = sophieId, AvatarUrl = "https://picsum.photos/200/300?random=18" },
            new Member { Id = jamesId, FullName = "James, Earl of Wessex", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(2007, 12, 17), FatherId = edwardId, MotherId = sophieId, AvatarUrl = "https://picsum.photos/200/300?random=19" },

            // Generation 3 (Children of William and Catherine, Harry and Meghan)
            new Member { Id = georgeId, FullName = "Prince George of Wales", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(2013, 7, 22), FatherId = williamId, MotherId = catherineId, AvatarUrl = "https://picsum.photos/200/300?random=20" },
            new Member { Id = charlotteId, FullName = "Princess Charlotte of Wales", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(2015, 5, 2), FatherId = williamId, MotherId = catherineId, AvatarUrl = "https://picsum.photos/200/300?random=21" },
            new Member { Id = louisId, FullName = "Prince Louis of Wales", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(2018, 4, 23), FatherId = williamId, MotherId = catherineId, AvatarUrl = "https://picsum.photos/200/300?random=22" },
            new Member { Id = lilibetId, FullName = "Princess Lilibet of Sussex", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(2021, 6, 4), FatherId = harryId, MotherId = meghanId, AvatarUrl = "https://picsum.photos/200/300?random=23" },
            new Member { Id = Guid.NewGuid(), FullName = "Archie Mountbatten-Windsor", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(2019, 5, 6), FatherId = harryId, MotherId = meghanId, AvatarUrl = "https://picsum.photos/200/300?random=24" },
        });
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

    public Task<int> CountMembersByFamilyIdAsync(Guid familyId)
    {
        return Task.FromResult(_items.Count(m => m.FamilyId == familyId));
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