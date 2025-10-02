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
        Guid elizabethIIId = Guid.Parse("be6eae23-4572-4bd3-ac8d-18f0fa6ab1fe");
        Guid philipId = Guid.Parse("afb9d4fb-cb0a-4af5-ad73-a5f15810dae7");
        Guid charlesIIIId = Guid.Parse("d81a0cce-3a8f-4abf-b622-5cee0b7406f4");
        Guid camillaId = Guid.Parse("43432be4-f04d-48a3-8932-e5099979efdb");
        Guid anneId = Guid.Parse("bc159782-68dc-4341-b24c-4ae5b7c9c477");
        Guid andrewId = Guid.Parse("b3cc383c-9151-4c78-b82f-c52395370009");
        Guid edwardId = Guid.Parse("243f3b40-f0ce-473b-9ce0-3483f104cad7");
        Guid williamId = Guid.Parse("dcbe63bf-dfbf-4278-9d1c-d82083831a50");
        Guid catherineId = Guid.Parse("0c0510bc-af3a-4ea4-a596-5c324a585ba1");
        Guid georgeId = Guid.Parse("1edce7e7-6e7a-47a4-820b-574eea14e14f");
        Guid charlotteId = Guid.Parse("c21d3e3e-9dd6-47f3-82aa-1323778c3a65");
        Guid louisId = Guid.Parse("8394229c-7ccb-4e7e-8151-fd4f587b4707");
        Guid harryId = Guid.Parse("605f56c4-6c0b-4c93-a0fa-8a12a53d201d");
        Guid meghanId = Guid.Parse("69188b7d-c117-4ed0-bbb4-6a52828f2555");
        Guid dianaId = Guid.Parse("e29a5466-558d-4248-9aa9-20a18784143b");
        Guid beatriceId = Guid.Parse("89d5d27c-0795-4e03-89d1-c5b5aeae2422");
        Guid eugenieId = Guid.Parse("a3e1b971-ba2c-4443-b8ee-68be58976b9d");
        Guid louiseId = Guid.Parse("a7dff2be-aca1-47ee-b46a-7f0156be8ee9");
        Guid jamesId = Guid.Parse("3a759b93-328c-47e7-ac28-cfeba234c789");
        Guid sarahId = Guid.Parse("304b4cdc-8097-4858-8914-0e13a3f2b8aa");
        Guid timothyId = Guid.Parse("d0d57733-32a2-4a74-b1b0-a6f56dc58310");
        Guid sophieId = Guid.Parse("99d68383-3ddd-43ca-885d-0116e708b20d");
        Guid lilibetId = Guid.Parse("edf86d00-c012-4795-9ab1-732968fd028c");
        Guid archieId = Guid.Parse("4c61f779-04ee-44af-a21a-154dffab94ef");

        _items.AddRange(new List<Member>
        {
            // Generation 0 (for context)
            new Member { Id = elizabethIIId, FirstName = "Queen", LastName = "Elizabeth II", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1926, 4, 21), DateOfDeath = new DateTime(2022, 9, 8), SpouseId = philipId, AvatarUrl = "https://picsum.photos/200/300?random=1" },
            new Member { Id = philipId, FirstName = "Prince", LastName = "Philip, Duke of Edinburgh", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1921, 6, 10), DateOfDeath = new DateTime(2021, 4, 9), SpouseId = elizabethIIId, AvatarUrl = "https://picsum.photos/200/300?random=2" },
            new Member { Id = dianaId, FirstName = "Diana,", LastName = "Princess of Wales", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1961, 7, 1), DateOfDeath = new DateTime(1997, 8, 31), SpouseId = charlesIIIId, AvatarUrl = "https://picsum.photos/200/300?random=3" },
            new Member { Id = camillaId, FirstName = "Camilla,", LastName = "Queen Consort", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1947, 7, 17), SpouseId = charlesIIIId, AvatarUrl = "https://picsum.photos/200/300?random=4" },
            new Member { Id = sarahId, FirstName = "Sarah", LastName = "Ferguson", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1959, 10, 15), SpouseId = andrewId, AvatarUrl = "https://picsum.photos/200/300?random=5" },
            new Member { Id = timothyId, FirstName = "Sir", LastName = "Timothy Laurence", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1955, 3, 1), SpouseId = anneId, AvatarUrl = "https://picsum.photos/200/300?random=6" },
            new Member { Id = sophieId, FirstName = "Sophie,", LastName = "Duchess of Edinburgh", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1965, 1, 20), SpouseId = edwardId, AvatarUrl = "https://picsum.photos/200/300?random=7" },
            new Member { Id = catherineId, FirstName = "Catherine,", LastName = "Princess of Wales", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1982, 1, 9), SpouseId = williamId, AvatarUrl = "https://picsum.photos/200/300?random=8" },
            new Member { Id = meghanId, FirstName = "Meghan,", LastName = "Duchess of Sussex", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1981, 8, 4), SpouseId = harryId, AvatarUrl = "https://picsum.photos/200/300?random=9" },


            // Generation 1 (Children of Elizabeth II and Philip)
            new Member { Id = charlesIIIId, FirstName = "King", LastName = "Charles III", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1948, 11, 14), FatherId = philipId, MotherId = elizabethIIId, SpouseId = camillaId, AvatarUrl = "https://picsum.photos/200/300?random=10" },
            new Member { Id = anneId, FirstName = "Anne,", LastName = "Princess Royal", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1950, 8, 15), FatherId = philipId, MotherId = elizabethIIId, SpouseId = timothyId, AvatarUrl = "https://picsum.photos/200/300?random=11" },
            new Member { Id = andrewId, FirstName = "Prince", LastName = "Andrew, Duke of York", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1960, 2, 19), FatherId = philipId, MotherId = elizabethIIId, SpouseId = sarahId, AvatarUrl = "https://picsum.photos/200/300?random=12" },
            new Member { Id = edwardId, FirstName = "Prince", LastName = "Edward, Duke of Edinburgh", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1964, 3, 10), FatherId = philipId, MotherId = elizabethIIId, SpouseId = sophieId, AvatarUrl = "https://picsum.photos/200/300?random=13" },

            // Generation 2 (Children of Charles III and Diana) and Andrew/Sarah, Edward/Sophie
            new Member { Id = williamId, FirstName = "Prince", LastName = "William, Prince of Wales", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1982, 6, 21), FatherId = charlesIIIId, MotherId = dianaId, SpouseId = catherineId, AvatarUrl = "https://picsum.photos/200/300?random=14" },
            new Member { Id = harryId, FirstName = "Prince", LastName = "Harry, Duke of Sussex", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(1984, 9, 15), FatherId = charlesIIIId, MotherId = dianaId, SpouseId = meghanId, AvatarUrl = "https://picsum.photos/200/300?random=15" },
            new Member { Id = beatriceId, FirstName = "Princess", LastName = "Beatrice of York", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1988, 8, 8), FatherId = andrewId, MotherId = sarahId, AvatarUrl = "https://picsum.photos/200/300?random=16" },
            new Member { Id = eugenieId, FirstName = "Princess", LastName = "Eugenie of York", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(1990, 3, 23), FatherId = andrewId, MotherId = sarahId, AvatarUrl = "https://picsum.photos/200/300?random=17" },
            new Member { Id = louiseId, FirstName = "Lady", LastName = "Louise Windsor", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(2003, 11, 8), FatherId = edwardId, MotherId = sophieId, AvatarUrl = "https://picsum.photos/200/300?random=18" },
            new Member { Id = jamesId, FirstName = "James,", LastName = "Earl of Wessex", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(2007, 12, 17), FatherId = edwardId, MotherId = sophieId, AvatarUrl = "https://picsum.photos/200/300?random=19" },

            // Generation 3 (Children of William and Catherine, Harry and Meghan)
            new Member { Id = georgeId, FirstName = "Prince", LastName = "George of Wales", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(2013, 7, 22), FatherId = williamId, MotherId = catherineId, AvatarUrl = "https://picsum.photos/200/300?random=20" },
            new Member { Id = charlotteId, FirstName = "Princess", LastName = "Charlotte of Wales", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(2015, 5, 2), FatherId = williamId, MotherId = catherineId, AvatarUrl = "https://picsum.photos/200/300?random=21" },
            new Member { Id = louisId, FirstName = "Prince", LastName = "Louis of Wales", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(2018, 4, 23), FatherId = williamId, MotherId = catherineId, AvatarUrl = "https://picsum.photos/200/300?random=22" },
            new Member { Id = lilibetId, FirstName = "Princess", LastName = "Lilibet of Sussex", FamilyId = royalFamilyId, Gender = "Female", DateOfBirth = new DateTime(2021, 6, 4), FatherId = harryId, MotherId = meghanId, AvatarUrl = "https://picsum.photos/200/300?random=23" },
            new Member { Id = archieId, FirstName = "Archie", LastName = "Mountbatten-Windsor", FamilyId = royalFamilyId, Gender = "Male", DateOfBirth = new DateTime(2019, 5, 6), FatherId = harryId, MotherId = meghanId, AvatarUrl = "https://picsum.photos/200/300?random=24" },
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