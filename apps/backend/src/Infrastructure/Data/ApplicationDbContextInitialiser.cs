using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Data;

public class ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context)
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger = logger;
    private readonly ApplicationDbContext _context = context;

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
        await Task.CompletedTask;

        // Seed Relations
        if (!_context.Relations.Any())
        {
            _context.Relations.AddRange(
                new Domain.Entities.Relation
                {
                    Id = "ong_noi",
                    Name = "Ông nội",
                    Type = Domain.Enums.RelationType.Blood,
                    Description = "Cha của cha bạn",
                    Lineage = Domain.Enums.RelationLineage.Noi,
                    SpecialRelation = false,
                    NamesByRegion = new Domain.Entities.NamesByRegion
                    {
                        North = "Ông nội",
                        Central = "Ông nội",
                        South = "Ông nội"
                    }
                },
                new Domain.Entities.Relation
                {
                    Id = "ong_ngoai",
                    Name = "Ông ngoại",
                    Type = Domain.Enums.RelationType.Blood,
                    Description = "Cha của mẹ bạn",
                    Lineage = Domain.Enums.RelationLineage.Ngoai,
                    SpecialRelation = false,
                    NamesByRegion = new Domain.Entities.NamesByRegion
                    {
                        North = "Ông ngoại",
                        Central = "Ông ngoại",
                        South = "Ông ngoại"
                    }
                },
                new Domain.Entities.Relation
                {
                    Id = "cha_nuoi",
                    Name = "Cha nuôi",
                    Type = Domain.Enums.RelationType.Adoption,
                    Description = "Người nhận con làm cha nuôi",
                    Lineage = Domain.Enums.RelationLineage.NoiNgoai,
                    SpecialRelation = true,
                    NamesByRegion = new Domain.Entities.NamesByRegion
                    {
                        North = "Cha nuôi",
                        Central = "Cha nuôi",
                        South = "Cha nuôi"
                    }
                },
                new Domain.Entities.Relation
                {
                    Id = "me_ke",
                    Name = "Mẹ kế",
                    Type = Domain.Enums.RelationType.InLaw,
                    Description = "Vợ của cha nhưng không phải mẹ ruột",
                    Lineage = Domain.Enums.RelationLineage.Noi,
                    SpecialRelation = true,
                    NamesByRegion = new Domain.Entities.NamesByRegion
                    {
                        North = "Mẹ kế",
                        Central = "Mẹ kế",
                        South = "Mẹ kế"
                    }
                },
                new Domain.Entities.Relation
                {
                    Id = "chu_noi",
                    Name = "Chú",
                    Type = Domain.Enums.RelationType.Blood,
                    Description = "Em trai của cha",
                    Lineage = Domain.Enums.RelationLineage.Noi,
                    SpecialRelation = false,
                    NamesByRegion = new Domain.Entities.NamesByRegion
                    {
                        North = "Chú",
                        Central = new string[] { "Củ", "Chế" },
                        South = "Cậu"
                    }
                },
                new Domain.Entities.Relation
                {
                    Id = "co_noi",
                    Name = "Cô",
                    Type = Domain.Enums.RelationType.Blood,
                    Description = "Em gái của cha",
                    Lineage = Domain.Enums.RelationLineage.Noi,
                    SpecialRelation = false,
                    NamesByRegion = new Domain.Entities.NamesByRegion
                    {
                        North = "Cô",
                        Central = new string[] { "Mợ", "Dì" },
                        South = "Dì"
                    }
                }
            );

            await _context.SaveChangesAsync();
        }
        // // Default data
        // // Seed, if necessary

        // // Seed UserProfiles
        // if (!_context.UserProfiles.Any())
        // {
        //     var userProfile1Id = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
        //     var userProfile2Id = Guid.Parse("b0eebc99-9c0b-4ef8-bb6d-6bb9bd380a22");

        //     // Create dummy User entities for the UserProfiles
        //     var user1 = new Domain.Entities.User("auth0|testuser1", "testuser1@example.com");
        //     user1.Id = Guid.NewGuid(); // Assign a new GUID for the User
        //     user1.Profile?.Update("auth0|testuser1", "testuser1@example.com", "Test User One", "", "", "", ""); // Update profile using the method
        //     user1.Preference?.Update(Domain.Enums.Theme.Light, Domain.Enums.Language.English); // Update preference using the method

        //     var user2 = new Domain.Entities.User("auth0|testuser2", "testuser2@example.com");
        //     user2.Id = Guid.NewGuid(); // Assign a new GUID for the User
        //     user2.Profile?.Update("auth0|testuser2", "testuser2@example.com", "Test User Two", "", "", "", ""); // Update profile using the method
        //     user2.Preference?.Update(Domain.Enums.Theme.Light, Domain.Enums.Language.English); // Update preference using the method

        //     _context.Users.AddRange(user1, user2);
        //     await _context.SaveChangesAsync();
        // }

        // if (!_context.Families.Any())
        // {
        //     Guid royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb");
        //     _context.Families.Add(new Domain.Entities.Family
        //     {
        //         Id = royalFamilyId,
        //         Name = "Royal Family",
        //         Code = "FAM-ROYAL",
        //         Description = "The British Royal Family, a prominent family with a rich history.",
        //         AvatarUrl = "https://i.pravatar.cc/150?img=3",
        //         Address = "Buckingham Palace, London, UK",
        //         Visibility = Domain.Enums.FamilyVisibility.Public.ToString(),
        //         TotalMembers = 19, // Based on the members added below
        //         Created = DateTime.UtcNow
        //     });

        //     // Add 20 more sample families
        //     for (int i = 1; i <= 20; i++)
        //     {
        //         _context.Families.Add(new Domain.Entities.Family
        //         {
        //             Id = Guid.NewGuid(),
        //             Name = $"Family {i}",
        //             Code = $"FAM-{i:D3}",
        //             Description = $"Description for Family {i}",
        //             AvatarUrl = $"https://i.pravatar.cc/150?img=3",
        //             Address = "Address" + i.ToString(),
        //             Visibility = Domain.Enums.FamilyVisibility.Public.ToString(),
        //             TotalMembers = 0,
        //             Created = DateTime.UtcNow
        //         });
        //     }

        //     var williamId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef");
        //     var catherineId = Guid.Parse("b2c3d4e5-f6a1-8901-2345-67890abcdef0");
        //     var georgeId = Guid.Parse("c3d4e5f6-a1b2-9012-3456-7890abcdef01");
        //     var elizabethIIId = Guid.Parse("d4e5f6a1-b2c3-0123-4567-890abcdef012");
        //     var charlotteId = Guid.Parse("e5f6a1b2-c3d4-1234-5678-90abcdef0123");
        //     var louisId = Guid.Parse("f6a1b2c3-d4e5-2345-6789-0abcdef01234");
        //     var harryId = Guid.Parse("a1b2c3d4-e5f6-3456-7890-1234567890ab");
        //     var meghanId = Guid.Parse("b2c3d4e5-f6a1-4567-8901-234567890abc");
        //     var archieId = Guid.Parse("c3d4e5f6-a1b2-5678-9012-34567890abcd");
        //     var lilibetMountbattenWindsorId = Guid.Parse("d4e5f6a1-b2c3-6789-0123-4567890abcde");
        //     var charlesIIIId = Guid.Parse("e5f6a1b2-c3d4-7890-1234-567890abcdef");
        //     var queenConsortId = Guid.Parse("f6a1b2c3-d4e5-8901-2345-67890abcdef0");
        //     var philipId = Guid.Parse("a1b2c3d4-e5f6-9012-3456-7890abcdef01");
        //     var dianaId = Guid.Parse("b2c3d4e5-f6a1-0123-4567-890abcdef012");
        //     var andrewId = Guid.Parse("c3d4e5f6-a1b2-1234-5678-90abcdef0123");
        //     var sarahId = Guid.Parse("d4e5f6a1-b2c3-2345-6789-0abcdef01234");
        //     var eugenieId = Guid.Parse("e5f6a1b2-c3d4-3456-7890-1234567890ab");
        //     var beatriceId = Guid.Parse("f6a1b2c3-d4e5-4567-8901-234567890abc");
        //     var lilibetSussexId = Guid.Parse("a1b2c3d4-e5f6-5678-9012-34567890abcd");

        //     var members = new List<Domain.Entities.Member>
        //     {
        //         new(royalFamilyId) { Id = williamId, Code = "MEM-WM", FirstName = "Prince", LastName = "William",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(1982, 6, 21), PlaceOfBirth = "London", Occupation = "Royal" },
        //         new(royalFamilyId) { Id = catherineId, Code = "MEM-CT", FirstName = "Catherine", LastName = "Middleton",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1982, 1, 9), PlaceOfBirth = "Reading", Occupation = "Royal" },
        //         new(royalFamilyId) { Id = georgeId, Code = "MEM-GG", FirstName = "Prince", LastName = "George",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(2013, 7, 22), PlaceOfBirth = "London" },
        //         new(royalFamilyId) { Id = elizabethIIId, Code = "MEM-EL", FirstName = "Queen", LastName = "Elizabeth II",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1926, 4, 21), DateOfDeath = new DateTime(2022, 9, 8), PlaceOfBirth = "London", PlaceOfDeath = "Balmoral", Occupation = "Monarch" },
        //         new(royalFamilyId) { Id = charlotteId, Code = "MEM-CH", FirstName = "Princess", LastName = "Charlotte",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(2015, 5, 2), PlaceOfBirth = "London" },
        //         new(royalFamilyId) { Id = louisId, Code = "MEM-LO", FirstName = "Prince", LastName = "Louis",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(2018, 4, 23), PlaceOfBirth = "London" },
        //         new(royalFamilyId) { Id = harryId, Code = "MEM-HA", FirstName = "Prince", LastName = "Harry",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(1984, 9, 15), PlaceOfBirth = "London", Occupation = "Royal" },
        //         new(royalFamilyId) { Id = meghanId, Code = "MEM-MM", FirstName = "Meghan", LastName = "Markle",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1981, 8, 4), PlaceOfBirth = "Los Angeles", Occupation = "Actress" },
        //         new(royalFamilyId) { Id = archieId, Code = "MEM-AM", FirstName = "Archie", LastName = "Mountbatten-Windsor",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(2019, 5, 6), PlaceOfBirth = "London" },
        //         new(royalFamilyId) { Id = lilibetMountbattenWindsorId, Code = "MEM-LM", FirstName = "Lilibet", LastName = "Mountbatten-Windsor",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(2021, 6, 4), PlaceOfBirth = "Santa Barbara" },
        //         new(royalFamilyId) { Id = charlesIIIId, Code = "MEM-KC", FirstName = "King", LastName = "Charles III",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(1948, 11, 14), PlaceOfBirth = "London", Occupation = "Monarch"},
        //         new(royalFamilyId) { Id = queenConsortId, Code = "MEM-QC", FirstName = "Queen", LastName = "Consort",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1947, 7, 17), PlaceOfBirth = "London", Occupation = "Royal" },
        //         new(royalFamilyId) { Id = philipId, Code = "MEM-PP", FirstName = "Prince", LastName = "Philip",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(1921, 6, 10), DateOfDeath = new DateTime(2021, 4, 9), PlaceOfBirth = "Corfu", PlaceOfDeath = "Windsor", Occupation = "Royal Consort" },
        //         new(royalFamilyId) { Id = dianaId, Code = "MEM-PD", FirstName = "Princess", LastName = "Diana",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1961, 7, 1), DateOfDeath = new DateTime(1997, 8, 31), PlaceOfBirth = "Sandringham", PlaceOfDeath = "Paris", Occupation = "Princess" },
        //         new(royalFamilyId) { Id = andrewId, Code = "MEM-PA", FirstName = "Prince", LastName = "Andrew",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar.png", Gender = Domain.Enums.Gender.Male.ToString(), DateOfBirth = new DateTime(1960, 2, 19), PlaceOfBirth = "London", Occupation = "Royal" },
        //         new(royalFamilyId) { Id = sarahId, Code = "MEM-SF", FirstName = "Sarah", LastName = "Ferguson",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1959, 10, 15), PlaceOfBirth = "London", Occupation = "Author" },
        //         new(royalFamilyId) { Id = eugenieId, Code = "MEM-EU", FirstName = "Princess", LastName = "Eugenie",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1990, 3, 23), PlaceOfBirth = "London", Occupation = "Royal" },
        //         new(royalFamilyId) { Id = beatriceId, Code = "MEM-BE", FirstName = "Princess", LastName = "Beatrice",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(1988, 8, 8), PlaceOfBirth = "London", Occupation = "Royal" },
        //         new(royalFamilyId) { Id = lilibetSussexId, Code = "MEM-LS", FirstName = "Princess", LastName = "Lilibet",  Created = DateTime.UtcNow, AvatarUrl = "https://www.w3schools.com/howto/img_avatar2.png", Gender = Domain.Enums.Gender.Female.ToString(), DateOfBirth = new DateTime(2021, 6, 4), PlaceOfBirth = "Santa Barbara" }
        //     };
        //     members.FirstOrDefault(e => e.Id == philipId)?.SetAsRoot();
        //     _context.Members.AddRange(members);

        //     _context.Events.AddRange(new List<Domain.Entities.Event>
        //     {
        //         // new Event(
        //         //     "Birth of Prince George",
        //         //     "EVT-B-PG",
        //         //     Domain.Enums.EventType.Birth,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The birth of Prince George of Wales.",
        //         //     StartDate = new DateTime(2013, 7, 22),
        //         //     Location = "St Mary's Hospital, London",
        //         // }.AddEventMember(members.First(m => m.Id == georgeId).Id),
        //         // new Event(
        //         //     "Marriage of William and Catherine",
        //         //     "EVT-M-WC",
        //         //     Domain.Enums.EventType.Marriage,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The marriage of Prince William, Duke of Cambridge, and Catherine Middleton.",
        //         //     StartDate = new DateTime(2011, 4, 29),
        //         //     Location = "Westminster Abbey, London",
        //         // }.AddEventMember(members.First(m => m.Id == williamId).Id)
        //         //  .AddEventMember(members.First(m => m.Id == catherineId).Id),
        //         // new Event(
        //         //     "Death of Queen Elizabeth II",
        //         //     "EVT-D-QE",
        //         //     Domain.Enums.EventType.Death,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The death of Queen Elizabeth II.",
        //         //     StartDate = new DateTime(2022, 9, 8),
        //         //     Location = "Balmoral Castle, Scotland",
        //         // }.AddEventMember(members.First(m => m.Id == elizabethIIId).Id),
        //         // new Event(
        //         //     "Birth of Princess Charlotte",
        //         //     "EVT-B-PC",
        //         //     Domain.Enums.EventType.Birth,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The birth of Princess Charlotte of Wales.",
        //         //     StartDate = new DateTime(2015, 5, 2),
        //         //     Location = "St Mary's Hospital, London",
        //         // }.AddEventMember(members.First(m => m.Id == charlotteId).Id),
        //         // new Event(
        //         //     "Birth of Prince Louis",
        //         //     "EVT-B-PL",
        //         //     Domain.Enums.EventType.Birth,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The birth of Prince Louis of Wales.",
        //         //     StartDate = new DateTime(2018, 4, 23),
        //         //     Location = "St Mary's Hospital, London",
        //         // }.AddEventMember(members.First(m => m.Id == louisId).Id),
        //         // new Event(
        //         //     "Marriage of Harry and Meghan",
        //         //     "EVT-M-HM",
        //         //     Domain.Enums.EventType.Marriage,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The marriage of Prince Harry and Meghan Markle.",
        //         //     StartDate = new DateTime(2018, 5, 19),
        //         //     Location = "St George's Chapel, Windsor Castle",
        //         // }.AddEventMember(members.First(m => m.Id == harryId).Id)
        //         //  .AddEventMember(members.First(m => m.Id == meghanId).Id),
        //         // new Event(
        //         //     "Birth of Archie Mountbatten-Windsor",
        //         //     "EVT-B-AM",
        //         //     Domain.Enums.EventType.Birth,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The birth of Archie Mountbatten-Windsor.",
        //         //     StartDate = new DateTime(2019, 5, 6),
        //         //     Location = "The Portland Hospital, London",
        //         // }.AddEventMember(members.First(m => m.Id == archieId).Id),
        //         // new Event(
        //         //     "Birth of Lilibet Mountbatten-Windsor",
        //         //     "EVT-B-LM",
        //         //     Domain.Enums.EventType.Birth,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The birth of Lilibet Mountbatten-Windsor.",
        //         //     StartDate = new DateTime(2021, 6, 4),
        //         //     Location = "Santa Barbara Cottage Hospital, California",
        //         // }.AddEventMember(members.First(m => m.Id == lilibetMountbattenWindsorId).Id),
        //         // new Event(
        //         //     "Queen's Platinum Jubilee",
        //         //     "EVT-QPJ",
        //         //     Domain.Enums.EventType.Other,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "Celebration of Queen Elizabeth II's 70 years on the throne.",
        //         //     StartDate = new DateTime(2022, 6, 2),
        //         //     EndDate = new DateTime(2022, 6, 5),
        //         //     Location = "United Kingdom",
        //         // },
        //         // new Event(
        //         //     "Coronation of King Charles III",
        //         //     "EVT-C-KC",
        //         //     Domain.Enums.EventType.Other,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The coronation of King Charles III and Queen Camilla.",
        //         //     StartDate = new DateTime(2023, 5, 6),
        //         //     Location = "Westminster Abbey, London",
        //         // }.AddEventMember(members.First(m => m.Id == charlesIIIId).Id)
        //         //  .AddEventMember(members.First(m => m.Id == queenConsortId).Id),
        //         // new Event(
        //         //     "Trooping the Colour 2023",
        //         //     "EVT-TTC",
        //         //     Domain.Enums.EventType.Other,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The King's official birthday parade.",
        //         //     StartDate = new DateTime(2023, 6, 17),
        //         //     Location = "London",
        //         // },
        //         // new Event(
        //         //     "Prince Philip's Funeral",
        //         //     "EVT-F-PP",
        //         //     Domain.Enums.EventType.Death,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The funeral of Prince Philip, Duke of Edinburgh.",
        //         //     StartDate = new DateTime(2021, 4, 17),
        //         //     Location = "St George's Chapel, Windsor Castle",
        //         // }.AddEventMember(members.First(m => m.Id == philipId).Id),
        //         // new Event(
        //         //     "Princess Diana's Funeral",
        //         //     "EVT-F-PD",
        //         //     Domain.Enums.EventType.Death,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The funeral of Diana, Princess of Wales.",
        //         //     StartDate = new DateTime(1997, 9, 6),
        //         //     Location = "Westminster Abbey, London",
        //         // }.AddEventMember(members.First(m => m.Id == dianaId).Id),
        //         // new Event(
        //         //     "Royal Wedding of Prince Andrew and Sarah Ferguson",
        //         //     "EVT-M-AS",
        //         //     Domain.Enums.EventType.Marriage,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The wedding of Prince Andrew and Sarah Ferguson.",
        //         //     StartDate = new DateTime(1986, 7, 23),
        //         //     Location = "Westminster Abbey, London",
        //         // }.AddEventMember(members.First(m => m.Id == andrewId).Id)
        //         //  .AddEventMember(members.First(m => m.Id == sarahId).Id),
        //         // new Event(
        //         //     "Royal Wedding of Princess Eugenie and Jack Brooksbank",
        //         //     "EVT-M-EJ",
        //         //     Domain.Enums.EventType.Marriage,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The wedding of Princess Eugenie of York and Jack Brooksbank.",
        //         //     StartDate = new DateTime(2018, 10, 12),
        //         //     Location = "St George's Chapel, Windsor Castle",
        //         // }.AddEventMember(members.First(m => m.Id == eugenieId).Id),
        //         // new Event(
        //         //     "Royal Wedding of Princess Beatrice and Edoardo Mapelli Mozzi",
        //         //     "EVT-M-BM",
        //         //     Domain.Enums.EventType.Marriage,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The wedding of Princess Beatrice of York and Edoardo Mapelli Mozzi.",
        //         //     StartDate = new DateTime(2020, 7, 17),
        //         //     Location = "Royal Chapel of All Saints, Royal Lodge, Windsor",
        //         // }.AddEventMember(members.First(m => m.Id == beatriceId).Id),
        //         // new Event(
        //         //     "Birth of Prince William",
        //         //     "EVT-B-PW",
        //         //     Domain.Enums.EventType.Birth,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The birth of Prince William, Prince of Wales.",
        //         //     StartDate = new DateTime(1982, 6, 21),
        //         //     Location = "St Mary's Hospital, London",
        //         // }.AddEventMember(members.First(m => m.Id == williamId).Id),
        //         // new Event(
        //         //     "Birth of Prince Harry",
        //         //     "EVT-B-PH",
        //         //     Domain.Enums.EventType.Birth,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The birth of Prince Harry, Duke of Sussex.",
        //         //     StartDate = new DateTime(1984, 9, 15),
        //         //     Location = "St Mary's Hospital, London",
        //         // }.AddEventMember(members.First(m => m.Id == harryId).Id),
        //         // new Event(
        //         //     "Birth of King Charles III",
        //         //     "EVT-B-KC",
        //         //     Domain.Enums.EventType.Birth,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The birth of King Charles III.",
        //         //     StartDate = new DateTime(1948, 11, 14),
        //         //     Location = "Buckingham Palace, London",
        //         // }.AddEventMember(members.First(m => m.Id == charlesIIIId).Id),
        //         // new Event(
        //         //     "Queen Elizabeth II's Coronation",
        //         //     "EVT-C-QE",
        //         //     Domain.Enums.EventType.Other,
        //         //     royalFamilyId
        //         // )
        //         // {
        //         //     Id = Guid.NewGuid(),
        //         //     Description = "The coronation of Queen Elizabeth II.",
        //         //     StartDate = new DateTime(1953, 6, 2),
        //         //     Location = "Westminster Abbey, London",
        //         // }.AddEventMember(members.First(m => m.Id == elizabethIIId).Id)
        //     });

        //     // Add Relationships
        //     _context.Relationships.AddRange(new List<Domain.Entities.Relationship>
        //     {
        //         // Spouses
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = philipId, TargetMemberId = elizabethIIId, Type = Domain.Enums.RelationshipType.Husband,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = williamId, TargetMemberId = catherineId, Type = Domain.Enums.RelationshipType.Husband,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = harryId, TargetMemberId = meghanId, Type = Domain.Enums.RelationshipType.Husband,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = charlesIIIId, TargetMemberId = queenConsortId, Type = Domain.Enums.RelationshipType.Husband,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = andrewId, TargetMemberId = sarahId, Type = Domain.Enums.RelationshipType.Husband,  },

        //         // Parent-Child: Philip & Elizabeth II -> Charles III
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = philipId, TargetMemberId = charlesIIIId, Type = Domain.Enums.RelationshipType.Father,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = elizabethIIId, TargetMemberId = charlesIIIId, Type = Domain.Enums.RelationshipType.Mother,  },

        //         // Parent-Child: Charles III & Diana -> William, Harry
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = charlesIIIId, TargetMemberId = williamId, Type = Domain.Enums.RelationshipType.Father,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = dianaId, TargetMemberId = williamId, Type = Domain.Enums.RelationshipType.Mother,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = charlesIIIId, TargetMemberId = harryId, Type = Domain.Enums.RelationshipType.Father,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = dianaId, TargetMemberId = harryId, Type = Domain.Enums.RelationshipType.Mother,  },

        //         // Parent-Child: William & Catherine -> George, Charlotte, Louis
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = williamId, TargetMemberId = georgeId, Type = Domain.Enums.RelationshipType.Father,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = catherineId, TargetMemberId = georgeId, Type = Domain.Enums.RelationshipType.Mother,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = williamId, TargetMemberId = charlotteId, Type = Domain.Enums.RelationshipType.Father,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = catherineId, TargetMemberId = charlotteId, Type = Domain.Enums.RelationshipType.Mother,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = williamId, TargetMemberId = louisId, Type = Domain.Enums.RelationshipType.Father,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = catherineId, TargetMemberId = louisId, Type = Domain.Enums.RelationshipType.Mother,  },

        //         // Parent-Child: Harry & Meghan -> Archie, Lilibet Mountbatten-Windsor
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = harryId, TargetMemberId = archieId, Type = Domain.Enums.RelationshipType.Father,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = meghanId, TargetMemberId = archieId, Type = Domain.Enums.RelationshipType.Mother,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = harryId, TargetMemberId = lilibetMountbattenWindsorId, Type = Domain.Enums.RelationshipType.Father,  },
        //         new(royalFamilyId) { Id = Guid.NewGuid(), SourceMemberId = meghanId, TargetMemberId = lilibetMountbattenWindsorId, Type = Domain.Enums.RelationshipType.Mother,  }
        //     });

        //     await _context.SaveChangesAsync();
        // }
    }
}
