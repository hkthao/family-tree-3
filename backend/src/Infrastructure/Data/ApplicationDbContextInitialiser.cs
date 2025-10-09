using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Data;

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsRelational())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default data
        // Seed, if necessary

        // Seed UserProfiles
        if (!_context.UserProfiles.Any())
        {
            var userProfile1Id = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var userProfile2Id = Guid.Parse("b0eebc99-9c0b-4ef8-bb6d-6bb9bd380a22");

            _context.UserProfiles.AddRange(new backend.Domain.Entities.UserProfile[]
            {
                new backend.Domain.Entities.UserProfile
                {
                    Id = userProfile1Id,
                    ExternalId = "auth0|testuser1",
                    Email = "testuser1@example.com",
                    Name = "Test User One",
                    Created = DateTime.UtcNow
                },
                new backend.Domain.Entities.UserProfile
                {
                    Id = userProfile2Id,
                    ExternalId = "auth0|testuser2",
                    Email = "testuser2@example.com",
                    Name = "Test User Two",
                    Created = DateTime.UtcNow
                }
            });
            await _context.SaveChangesAsync();
        }

        if (!_context.Families.Any())
        {
            Guid royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb");
            _context.Families.Add(new backend.Domain.Entities.Family
            {
                Id = royalFamilyId,
                Name = "Royal Family",
                Description = "The British Royal Family, a prominent family with a rich history.",
                AvatarUrl = "https://i.pravatar.cc/150?img=3",
                Address = "Buckingham Palace, London, UK",
                Visibility = backend.Domain.Enums.FamilyVisibility.Public.ToString(),
                TotalMembers = 19, // Based on the members added below
                Created = DateTime.UtcNow
            });

            // Add 20 more sample families
            for (int i = 1; i <= 20; i++)
            {
                _context.Families.Add(new backend.Domain.Entities.Family
                {
                    Id = Guid.NewGuid(),
                    Name = $"Family {i}",
                    Description = $"Description for Family {i}",
                    AvatarUrl = $"https://i.pravatar.cc/150?img=3",
                    Address = "Address" + i.ToString(),
                    Visibility = backend.Domain.Enums.FamilyVisibility.Public.ToString(),
                    TotalMembers = 0,
                    Created = DateTime.UtcNow
                });
            }

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
                new backend.Domain.Entities.Member { Id = williamId, FirstName = "Prince", LastName = "William", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = backend.Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(1982, 6, 21), PlaceOfBirth = "London", Occupation = "Royal" },
                new backend.Domain.Entities.Member { Id = catherineId, FirstName = "Catherine", LastName = "Middleton", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = backend.Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1982, 1, 9), PlaceOfBirth = "Reading", Occupation = "Royal" },
                new backend.Domain.Entities.Member { Id = georgeId, FirstName = "Prince", LastName = "George", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = backend.Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(2013, 7, 22), PlaceOfBirth = "London" },
                new backend.Domain.Entities.Member { Id = elizabethIIId, FirstName = "Queen", LastName = "Elizabeth II", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = backend.Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1926, 4, 21), DateOfDeath = new DateTime(2022, 9, 8), PlaceOfBirth = "London", PlaceOfDeath = "Balmoral", Occupation = "Monarch" },
                new backend.Domain.Entities.Member { Id = charlotteId, FirstName = "Princess", LastName = "Charlotte", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = backend.Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(2015, 5, 2), PlaceOfBirth = "London" },
                new backend.Domain.Entities.Member { Id = louisId, FirstName = "Prince", LastName = "Louis", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = backend.Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(2018, 4, 23), PlaceOfBirth = "London" },
                new backend.Domain.Entities.Member { Id = harryId, FirstName = "Prince", LastName = "Harry", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = backend.Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(1984, 9, 15), PlaceOfBirth = "London", Occupation = "Royal" },
                new backend.Domain.Entities.Member { Id = meghanId, FirstName = "Meghan", LastName = "Markle", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = backend.Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1981, 8, 4), PlaceOfBirth = "Los Angeles", Occupation = "Actress" },
                new backend.Domain.Entities.Member { Id = archieId, FirstName = "Archie", LastName = "Mountbatten-Windsor", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = backend.Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(2019, 5, 6), PlaceOfBirth = "London" },
                new backend.Domain.Entities.Member { Id = lilibetMountbattenWindsorId, FirstName = "Lilibet", LastName = "Mountbatten-Windsor", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = backend.Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(2021, 6, 4), PlaceOfBirth = "Santa Barbara" },
                new backend.Domain.Entities.Member { Id = charlesIIIId, FirstName = "King", LastName = "Charles III", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = backend.Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(1948, 11, 14), PlaceOfBirth = "London", Occupation = "Monarch"},
                new backend.Domain.Entities.Member { Id = queenConsortId, FirstName = "Queen", LastName = "Consort", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = backend.Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1947, 7, 17), PlaceOfBirth = "London", Occupation = "Royal" },
                new backend.Domain.Entities.Member { Id = philipId, FirstName = "Prince", LastName = "Philip", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = backend.Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(1921, 6, 10), DateOfDeath = new DateTime(2021, 4, 9), PlaceOfBirth = "Corfu", PlaceOfDeath = "Windsor", Occupation = "Royal Consort", IsRoot = true },
                new backend.Domain.Entities.Member { Id = dianaId, FirstName = "Princess", LastName = "Diana", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = backend.Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1961, 7, 1), DateOfDeath = new DateTime(1997, 8, 31), PlaceOfBirth = "Sandringham", PlaceOfDeath = "Paris", Occupation = "Princess" },
                new backend.Domain.Entities.Member { Id = andrewId, FirstName = "Prince", LastName = "Andrew", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = backend.Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(1960, 2, 19), PlaceOfBirth = "London", Occupation = "Royal" },
                new backend.Domain.Entities.Member { Id = sarahId, FirstName = "Sarah", LastName = "Ferguson", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = backend.Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1959, 10, 15), PlaceOfBirth = "London", Occupation = "Author" },
                new backend.Domain.Entities.Member { Id = eugenieId, FirstName = "Princess", LastName = "Eugenie", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = backend.Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1990, 3, 23), PlaceOfBirth = "London", Occupation = "Royal" },
                new backend.Domain.Entities.Member { Id = beatriceId, FirstName = "Princess", LastName = "Beatrice", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = backend.Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1988, 8, 8), PlaceOfBirth = "London", Occupation = "Royal" },
                new backend.Domain.Entities.Member { Id = lilibetSussexId, FirstName = "Princess", LastName = "Lilibet", FamilyId = royalFamilyId, Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = backend.Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(2021, 6, 4), PlaceOfBirth = "Santa Barbara" }
            };
            _context.Members.AddRange(members);

            _context.Events.AddRange(new List<backend.Domain.Entities.Event>
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

            // Add Relationships
            _context.Relationships.AddRange(new List<backend.Domain.Entities.Relationship>
            {
                // Spouses
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = philipId, TargetMemberId = elizabethIIId, Type = backend.Domain.Enums.RelationshipType.Husband },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = elizabethIIId, TargetMemberId = philipId, Type = backend.Domain.Enums.RelationshipType.Wife },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = williamId, TargetMemberId = catherineId, Type = backend.Domain.Enums.RelationshipType.Husband },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = catherineId, TargetMemberId = williamId, Type = backend.Domain.Enums.RelationshipType.Wife },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = harryId, TargetMemberId = meghanId, Type = backend.Domain.Enums.RelationshipType.Husband },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = meghanId, TargetMemberId = harryId, Type = backend.Domain.Enums.RelationshipType.Wife },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = charlesIIIId, TargetMemberId = queenConsortId, Type = backend.Domain.Enums.RelationshipType.Husband },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = queenConsortId, TargetMemberId = charlesIIIId, Type = backend.Domain.Enums.RelationshipType.Wife },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = andrewId, TargetMemberId = sarahId, Type = backend.Domain.Enums.RelationshipType.Husband },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = sarahId, TargetMemberId = andrewId, Type = backend.Domain.Enums.RelationshipType.Wife },

                // Parent-Child: Philip & Elizabeth II -> Charles III
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = philipId, TargetMemberId = charlesIIIId, Type = backend.Domain.Enums.RelationshipType.Father },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = elizabethIIId, TargetMemberId = charlesIIIId, Type = backend.Domain.Enums.RelationshipType.Mother },

                // Parent-Child: Charles III & Diana -> William, Harry
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = charlesIIIId, TargetMemberId = williamId, Type = backend.Domain.Enums.RelationshipType.Father },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = dianaId, TargetMemberId = williamId, Type = backend.Domain.Enums.RelationshipType.Mother },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = charlesIIIId, TargetMemberId = harryId, Type = backend.Domain.Enums.RelationshipType.Father },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = dianaId, TargetMemberId = harryId, Type = backend.Domain.Enums.RelationshipType.Mother },

                // Parent-Child: William & Catherine -> George, Charlotte, Louis
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = williamId, TargetMemberId = georgeId, Type = backend.Domain.Enums.RelationshipType.Father },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = catherineId, TargetMemberId = georgeId, Type = backend.Domain.Enums.RelationshipType.Mother },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = williamId, TargetMemberId = charlotteId, Type = backend.Domain.Enums.RelationshipType.Father },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = catherineId, TargetMemberId = charlotteId, Type = backend.Domain.Enums.RelationshipType.Mother },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = williamId, TargetMemberId = louisId, Type = backend.Domain.Enums.RelationshipType.Father },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = catherineId, TargetMemberId = louisId, Type = backend.Domain.Enums.RelationshipType.Mother },

                // Parent-Child: Harry & Meghan -> Archie, Lilibet Mountbatten-Windsor
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = harryId, TargetMemberId = archieId, Type = backend.Domain.Enums.RelationshipType.Father },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = meghanId, TargetMemberId = archieId, Type = backend.Domain.Enums.RelationshipType.Mother },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = harryId, TargetMemberId = lilibetMountbattenWindsorId, Type = backend.Domain.Enums.RelationshipType.Father },
                new backend.Domain.Entities.Relationship { Id = Guid.NewGuid(), SourceMemberId = meghanId, TargetMemberId = lilibetMountbattenWindsorId, Type = backend.Domain.Enums.RelationshipType.Mother },
            });

            await _context.SaveChangesAsync();
        }

        // Seed UserActivities
        if (!_context.UserActivities.Any())
        {
            var userProfile1Id = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var userProfile2Id = Guid.Parse("b0eebc99-9c0b-4ef8-bb6d-6bb9bd380a22");
            var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb");
            var williamId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef");

            _context.UserActivities.AddRange(new backend.Domain.Entities.UserActivity[]
            {
                new backend.Domain.Entities.UserActivity
                {
                    UserProfileId = userProfile1Id,
                    ActionType = backend.Domain.Enums.UserActionType.Login,
                    TargetType = backend.Domain.Enums.TargetType.UserProfile,
                    TargetId = userProfile1Id.ToString(),
                    ActivitySummary = "User Test User One logged in.",
                    GroupId= Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"),
                    Created = DateTime.UtcNow.AddDays(-5)
                },
                new backend.Domain.Entities.UserActivity
                {
                    UserProfileId = userProfile1Id,
                    ActionType = backend.Domain.Enums.UserActionType.CreateFamily,
                    TargetType = backend.Domain.Enums.TargetType.Family,
                    TargetId = royalFamilyId.ToString(),
                    ActivitySummary = "User Test User One created Royal Family.",
                    GroupId= Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"),
                    Created = DateTime.UtcNow.AddDays(-4)
                },
                new backend.Domain.Entities.UserActivity
                {
                    UserProfileId = userProfile2Id,
                    ActionType = backend.Domain.Enums.UserActionType.Login,
                    TargetType = backend.Domain.Enums.TargetType.UserProfile,
                    TargetId = userProfile2Id.ToString(),
                    ActivitySummary = "User Test User Two logged in.",
                    Created = DateTime.UtcNow.AddDays(-3)
                },
                new backend.Domain.Entities.UserActivity
                {
                    UserProfileId = userProfile1Id,
                    ActionType = backend.Domain.Enums.UserActionType.CreateMember,
                    TargetType = backend.Domain.Enums.TargetType.Member,
                    TargetId = williamId.ToString(),
                    ActivitySummary = "User Test User One created member Prince William.",
                    GroupId= Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"),
                    Created = DateTime.UtcNow.AddDays(-2)
                }
            });
            await _context.SaveChangesAsync();
        }
    }
}
