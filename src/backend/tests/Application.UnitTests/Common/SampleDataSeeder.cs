using backend.Infrastructure.Data;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Common;

public static class SampleDataSeeder
{
    public static void SeedSampleData(ApplicationDbContext context)
    {
        Guid royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb");
        if (context.Families.Any(f => f.Id == royalFamilyId))
            return;

        context.Families.Add(new backend.Domain.Entities.Family { Id = royalFamilyId, Name = "Royal Family", Code = "ROYAL", Created = DateTime.UtcNow });

        var williamId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef");
        var catherineId = Guid.Parse("b2c3d4e5-f6a1-8901-2345-67890abcdef0");
        var georgeId = Guid.Parse("c3d4e5f6-a1b2-9012-3456-7890abcdef01");
        var elizabethIIId = Guid.Parse("d4e5f6a1-b2c3-0123-4567-890abcdef012");
        var charlotteId = Guid.Parse("e5f6a1b2-c3d4-1234-5678-90abcdef0123");
        var louisId = Guid.Parse("f6a1b2c3-d4e5-2345-6789-0abcdef01234");
        var harryId = Guid.Parse("a1b2c3d4-e5f6-3456-7890-1234567890ab");
        var meghanId = Guid.Parse("b2c3d4e5-f6a1-4567-8901-234567890abc");
        var archieId = Guid.Parse("c3d4e5f6-a1b2-5678-9012-34567890abcd");
        var lilibetMountbattenWindsorId = Guid.Parse("d4e5f6a1-b2c3-6789-0123-4567890abcde");
        var charlesIIIId = Guid.Parse("e5f6a1b2-c3d4-7890-1234-567890abcdef");
        var queenConsortId = Guid.Parse("f6a1b2c3-d4e5-8901-2345-67890abcdef0");
        var philipId = Guid.Parse("a1b2c3d4-e5f6-9012-3456-7890abcdef01");
        var dianaId = Guid.Parse("b2c3d4e5-f6a1-0123-4567-890abcdef012");
        var andrewId = Guid.Parse("c3d4e5f6-a1b2-1234-5678-90abcdef0123");
        var sarahId = Guid.Parse("d4e5f6a1-b2c3-2345-6789-0abcdef01234");
        var eugenieId = Guid.Parse("e5f6a1b2-c3d4-3456-7890-1234567890ab");
        var beatriceId = Guid.Parse("f6a1b2c3-d4e5-4567-8901-234567890abc");
        var lilibetSussexId = Guid.Parse("a1b2c3d4-e5f6-5678-9012-34567890abcd");

        var members = new List<backend.Domain.Entities.Member>
        {
            new backend.Domain.Entities.Member { Id = williamId, FirstName = "Prince", LastName = "William, Prince of Wales", Code = "M001", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Male.ToString() },
            new backend.Domain.Entities.Member { Id = catherineId, FirstName = "Catherine,", LastName = "Princess of Wales", Code = "M002", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Female.ToString() },
            new backend.Domain.Entities.Member { Id = georgeId, FirstName = "Prince", LastName = "George of Wales", Code = "M003", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Male.ToString() },
            new backend.Domain.Entities.Member { Id = elizabethIIId, FirstName = "Queen", LastName = "Elizabeth II", Code = "M004", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Female.ToString() },
            new backend.Domain.Entities.Member { Id = charlotteId, FirstName = "Princess", LastName = "Charlotte of Wales", Code = "M005", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Female.ToString() },
            new backend.Domain.Entities.Member { Id = louisId, FirstName = "Prince", LastName = "Louis of Wales", Code = "M006", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Male.ToString() },
            new backend.Domain.Entities.Member { Id = harryId, FirstName = "Prince", LastName = "Harry, Duke of Sussex", Code = "M007", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Male.ToString() },
            new backend.Domain.Entities.Member { Id = meghanId, FirstName = "Meghan,", LastName = "Duchess of Sussex", Code = "M008", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Female.ToString() },
            new backend.Domain.Entities.Member { Id = archieId, FirstName = "Archie", LastName = "Mountbatten-Windsor", Code = "M009", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Male.ToString() },
            new backend.Domain.Entities.Member { Id = lilibetMountbattenWindsorId, FirstName = "Lilibet", LastName = "Mountbatten-Windsor", Code = "M010", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Female.ToString() },
            new backend.Domain.Entities.Member { Id = charlesIIIId, FirstName = "King", LastName = "Charles III", Code = "M011", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Male.ToString() },
            new backend.Domain.Entities.Member { Id = queenConsortId, FirstName = "Queen", LastName = "Consort", Code = "M012", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Female.ToString() },
            new backend.Domain.Entities.Member { Id = philipId, FirstName = "Prince", LastName = "Philip, Duke of Edinburgh", Code = "M013", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Male.ToString() },
            new backend.Domain.Entities.Member { Id = dianaId, FirstName = "Princess", LastName = "of Wales", Code = "M014", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Female.ToString() }, // Diana
            new backend.Domain.Entities.Member { Id = andrewId, FirstName = "Prince", LastName = "Andrew, Duke of York", Code = "M015", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Male.ToString() },
            new backend.Domain.Entities.Member { Id = sarahId, FirstName = "Sarah", LastName = "Ferguson", Code = "M016", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Female.ToString() },
            new backend.Domain.Entities.Member { Id = eugenieId, FirstName = "Princess", LastName = "Eugenie of York", Code = "M017", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Female.ToString() },
            new backend.Domain.Entities.Member { Id = beatriceId, FirstName = "Princess", LastName = "Beatrice of York", Code = "M018", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Female.ToString() },
            new backend.Domain.Entities.Member { Id = lilibetSussexId, FirstName = "Princess", LastName = "Lilibet of Sussex", Code = "M019", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Gender = backend.Domain.Enums.Gender.Female.ToString() }
        };
        context.Members.AddRange(members);

        context.Events.AddRange(new List<backend.Domain.Entities.Event>
        {
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Prince George",
                Code = "E001",
                Description = "The birth of Prince George of Wales.",
                StartDate = new DateTime(2013, 7, 22),
                Location = "St Mary's Hospital, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Birth,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == georgeId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Marriage of William and Catherine",
                Code = "E002",
                Description = "The marriage of Prince William, Duke of Cambridge, and Catherine Middleton.",
                StartDate = new DateTime(2011, 4, 29),
                Location = "Westminster Abbey, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Marriage,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == williamId), members.First(m => m.Id == catherineId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Death of Queen Elizabeth II",
                Code = "E003",
                Description = "The death of Queen Elizabeth II.",
                StartDate = new DateTime(2022, 9, 8),
                Location = "Balmoral Castle, Scotland",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Death,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == elizabethIIId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Princess Charlotte",
                Code = "E004",
                Description = "The birth of Princess Charlotte of Wales.",
                StartDate = new DateTime(2015, 5, 2),
                Location = "St Mary's Hospital, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Birth,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == charlotteId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Prince Louis",
                Code = "E005",
                Description = "The birth of Prince Louis of Wales.",
                StartDate = new DateTime(2018, 4, 23),
                Location = "St Mary's Hospital, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Birth,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == louisId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Marriage of Harry and Meghan",
                Code = "E006",
                Description = "The marriage of Prince Harry and Meghan Markle.",
                StartDate = new DateTime(2018, 5, 19),
                Location = "St George's Chapel, Windsor Castle",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Marriage,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == harryId), members.First(m => m.Id == meghanId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Archie Mountbatten-Windsor",
                Code = "E007",
                Description = "The birth of Archie Mountbatten-Windsor.",
                StartDate = new DateTime(2019, 5, 6),
                Location = "The Portland Hospital, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Birth,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == archieId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Lilibet Mountbatten-Windsor",
                Code = "E008",
                Description = "The birth of Lilibet Mountbatten-Windsor.",
                StartDate = new DateTime(2021, 6, 4),
                Location = "Santa Barbara Cottage Hospital, California",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Birth,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == lilibetMountbattenWindsorId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Queen's Platinum Jubilee",
                Code = "E009",
                Description = "Celebration of Queen Elizabeth II's 70 years on the throne.",
                StartDate = new DateTime(2022, 6, 2),
                EndDate = new DateTime(2022, 6, 5),
                Location = "United Kingdom",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Other
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Coronation of King Charles III",
                Code = "E010",
                Description = "The coronation of King Charles III and Queen Camilla.",
                StartDate = new DateTime(2023, 5, 6),
                Location = "Westminster Abbey, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Other,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == charlesIIIId), members.First(m => m.Id == queenConsortId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Trooping the Colour 2023",
                Code = "E011",
                Description = "The King's official birthday parade.",
                StartDate = new DateTime(2023, 6, 17),
                Location = "London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Other
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Prince Philip's Funeral",
                Code = "E012",
                Description = "The funeral of Prince Philip, Duke of Edinburgh.",
                StartDate = new DateTime(2021, 4, 17),
                Location = "St George's Chapel, Windsor Castle",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Death,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == philipId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Princess Diana's Funeral",
                Code = "E013",
                Description = "The funeral of Diana, Princess of Wales.",
                StartDate = new DateTime(1997, 9, 6),
                Location = "Westminster Abbey, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Death,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == dianaId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Royal Wedding of Prince Andrew and Sarah Ferguson",
                Code = "E014",
                Description = "The wedding of Prince Andrew and Sarah Ferguson.",
                StartDate = new DateTime(1986, 7, 23),
                Location = "Westminster Abbey, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Marriage,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == andrewId), members.First(m => m.Id == sarahId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Royal Wedding of Princess Eugenie and Jack Brooksbank",
                Code = "E015",
                Description = "The wedding of Princess Eugenie of York and Jack Brooksbank.",
                StartDate = new DateTime(2018, 10, 12),
                Location = "St George's Chapel, Windsor Castle",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Marriage,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == eugenieId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Royal Wedding of Princess Beatrice and Edoardo Mapelli Mozzi",
                Code = "E016",
                Description = "The wedding of Princess Beatrice of York and Edoardo Mapelli Mozzi.",
                StartDate = new DateTime(2020, 7, 17),
                Location = "Royal Chapel of All Saints, Royal Lodge, Windsor",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Marriage,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == beatriceId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Prince William",
                Code = "E017",
                Description = "The birth of Prince William, Prince of Wales.",
                StartDate = new DateTime(1982, 6, 21),
                Location = "St Mary's Hospital, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Birth,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == williamId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Prince Harry",
                Code = "E018",
                Description = "The birth of Prince Harry, Duke of Sussex.",
                StartDate = new DateTime(1984, 9, 15),
                Location = "St Mary's Hospital, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Birth,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == harryId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of King Charles III",
                Code = "E019",
                Description = "The birth of King Charles III.",
                StartDate = new DateTime(1948, 11, 14),
                Location = "Buckingham Palace, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Birth,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == charlesIIIId) }
            },
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Queen Elizabeth II's Coronation",
                Code = "E020",
                Description = "The coronation of Queen Elizabeth II.",
                StartDate = new DateTime(1953, 6, 2),
                Location = "Westminster Abbey, London",
                FamilyId = royalFamilyId,
                Type = backend.Domain.Enums.EventType.Other,
                RelatedMembers = new List<backend.Domain.Entities.Member> { members.First(m => m.Id == elizabethIIId) }
            }
        });

        context.SaveChanges();
    }
}
