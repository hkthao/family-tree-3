using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Common;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(bool seedData = false)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        if (seedData)
            SeedSampleData(context);
        return context;
    }

    public static void Destroy(ApplicationDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }

    private static void SeedSampleData(ApplicationDbContext context)
    {
        Guid royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb");
        if (context.Families.Any(f => f.Id == royalFamilyId))
            return;

        context.Families.Add(new backend.Domain.Entities.Family { Id = royalFamilyId, Name = "Royal Family", Created = DateTime.UtcNow });

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
            new backend.Domain.Entities.Member { Id = williamId, FirstName = "Prince", LastName = "William, Prince of Wales", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = catherineId, FirstName = "Catherine,", LastName = "Princess of Wales", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = georgeId, FirstName = "Prince", LastName = "George of Wales", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = elizabethIIId, FirstName = "Queen", LastName = "Elizabeth II", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = charlotteId, FirstName = "Princess", LastName = "Charlotte of Wales", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = louisId, FirstName = "Prince", LastName = "Louis of Wales", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = harryId, FirstName = "Prince", LastName = "Harry, Duke of Sussex", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = meghanId, FirstName = "Meghan,", LastName = "Duchess of Sussex", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = archieId, FirstName = "Archie", LastName = "Mountbatten-Windsor", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = lilibetMountbattenWindsorId, FirstName = "Lilibet", LastName = "Mountbatten-Windsor", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = charlesIIIId, FirstName = "King", LastName = "Charles III", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = queenConsortId, FirstName = "Queen", LastName = "Consort", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = philipId, FirstName = "Prince", LastName = "Philip, Duke of Edinburgh", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = dianaId, FirstName = "Princess", LastName = "of Wales", FamilyId = royalFamilyId, Created = DateTime.UtcNow }, // Diana
            new backend.Domain.Entities.Member { Id = andrewId, FirstName = "Prince", LastName = "Andrew, Duke of York", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = sarahId, FirstName = "Sarah", LastName = "Ferguson", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = eugenieId, FirstName = "Princess", LastName = "Eugenie of York", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = beatriceId, FirstName = "Princess", LastName = "Beatrice of York", FamilyId = royalFamilyId, Created = DateTime.UtcNow },
            new backend.Domain.Entities.Member { Id = lilibetSussexId, FirstName = "Princess", LastName = "Lilibet of Sussex", FamilyId = royalFamilyId, Created = DateTime.UtcNow }
        };
        context.Members.AddRange(members);

        context.Events.AddRange(new List<backend.Domain.Entities.Event>
        {
            new backend.Domain.Entities.Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Prince George",
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