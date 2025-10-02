using backend.Domain.Entities;
using backend.Application.Common.Interfaces;
using backend.Domain.Enums;

namespace backend.Infrastructure.Persistence;

public class InMemoryEventRepository : InMemoryRepository<Event>, IEventRepository
{
    public InMemoryEventRepository(IMemberRepository memberRepository)
    {
        Guid royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb");

        // Get member Guids from InMemoryMemberRepository
        var members = memberRepository.GetAllAsync().Result;
        var williamId = members.First(m => m.FirstName == "Prince" && m.LastName == "William, Prince of Wales").Id;
        var catherineId = members.First(m => m.FirstName == "Catherine," && m.LastName == "Princess of Wales").Id;
        var georgeId = members.First(m => m.FirstName == "Prince" && m.LastName == "George of Wales").Id;
        var elizabethIIId = members.First(m => m.FirstName == "Queen" && m.LastName == "Elizabeth II").Id;

        _items.AddRange(new List<Event>
        {
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Prince George",
                Description = "The birth of Prince George of Wales.",
                StartDate = new DateTime(2013, 7, 22),
                Location = "St Mary's Hospital, London",
                FamilyId = royalFamilyId,
                Type = EventType.Birth,
                RelatedMembers = new List<Member> { members.First(m => m.Id == georgeId) }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Marriage of William and Catherine",
                Description = "The marriage of Prince William, Duke of Cambridge, and Catherine Middleton.",
                StartDate = new DateTime(2011, 4, 29),
                Location = "Westminster Abbey, London",
                FamilyId = royalFamilyId,
                Type = EventType.Marriage,
                RelatedMembers = new List<Member> { members.First(m => m.Id == williamId), members.First(m => m.Id == catherineId) }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Death of Queen Elizabeth II",
                Description = "The death of Queen Elizabeth II.",
                StartDate = new DateTime(2022, 9, 8),
                Location = "Balmoral Castle, Scotland",
                FamilyId = royalFamilyId,
                Type = EventType.Death,
                RelatedMembers = new List<Member> { members.First(m => m.Id == elizabethIIId) }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Princess Charlotte",
                Description = "The birth of Princess Charlotte of Wales.",
                StartDate = new DateTime(2015, 5, 2),
                Location = "St Mary's Hospital, London",
                FamilyId = royalFamilyId,
                Type = EventType.Birth,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Charlotte of Wales") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Prince Louis",
                Description = "The birth of Prince Louis of Wales.",
                StartDate = new DateTime(2018, 4, 23),
                Location = "St Mary's Hospital, London",
                FamilyId = royalFamilyId,
                Type = EventType.Birth,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Louis of Wales") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Marriage of Harry and Meghan",
                Description = "The marriage of Prince Harry and Meghan Markle.",
                StartDate = new DateTime(2018, 5, 19),
                Location = "St George's Chapel, Windsor Castle",
                FamilyId = royalFamilyId,
                Type = EventType.Marriage,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Harry, Duke of Sussex"), members.First(m => m.LastName == "Meghan, Duchess of Sussex") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Archie Mountbatten-Windsor",
                Description = "The birth of Archie Mountbatten-Windsor.",
                StartDate = new DateTime(2019, 5, 6),
                Location = "The Portland Hospital, London",
                FamilyId = royalFamilyId,
                Type = EventType.Birth,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Mountbatten-Windsor") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Lilibet Mountbatten-Windsor",
                Description = "The birth of Lilibet Mountbatten-Windsor.",
                StartDate = new DateTime(2021, 6, 4),
                Location = "Santa Barbara Cottage Hospital, California",
                FamilyId = royalFamilyId,
                Type = EventType.Birth,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Lilibet of Sussex") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Queen's Platinum Jubilee",
                Description = "Celebration of Queen Elizabeth II's 70 years on the throne.",
                StartDate = new DateTime(2022, 6, 2),
                EndDate = new DateTime(2022, 6, 5),
                Location = "United Kingdom",
                FamilyId = royalFamilyId,
                Type = EventType.Other
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Coronation of King Charles III",
                Description = "The coronation of King Charles III and Queen Camilla.",
                StartDate = new DateTime(2023, 5, 6),
                Location = "Westminster Abbey, London",
                FamilyId = royalFamilyId,
                Type = EventType.Other,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Charles III"), members.First(m => m.LastName == "Queen Consort") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Trooping the Colour 2023",
                Description = "The King's official birthday parade.",
                StartDate = new DateTime(2023, 6, 17),
                Location = "London",
                FamilyId = royalFamilyId,
                Type = EventType.Other
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Prince Philip's Funeral",
                Description = "The funeral of Prince Philip, Duke of Edinburgh.",
                StartDate = new DateTime(2021, 4, 17),
                Location = "St George's Chapel, Windsor Castle",
                FamilyId = royalFamilyId,
                Type = EventType.Death,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Philip, Duke of Edinburgh") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Princess Diana's Funeral",
                Description = "The funeral of Diana, Princess of Wales.",
                StartDate = new DateTime(1997, 9, 6),
                Location = "Westminster Abbey, London",
                FamilyId = royalFamilyId,
                Type = EventType.Death,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Princess of Wales") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Royal Wedding of Prince Andrew and Sarah Ferguson",
                Description = "The wedding of Prince Andrew and Sarah Ferguson.",
                StartDate = new DateTime(1986, 7, 23),
                Location = "Westminster Abbey, London",
                FamilyId = royalFamilyId,
                Type = EventType.Marriage,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Andrew, Duke of York"), members.First(m => m.LastName == "Ferguson") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Royal Wedding of Princess Eugenie and Jack Brooksbank",
                Description = "The wedding of Princess Eugenie of York and Jack Brooksbank.",
                StartDate = new DateTime(2018, 10, 12),
                Location = "St George's Chapel, Windsor Castle",
                FamilyId = royalFamilyId,
                Type = EventType.Marriage,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Eugenie of York") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Royal Wedding of Princess Beatrice and Edoardo Mapelli Mozzi",
                Description = "The wedding of Princess Beatrice of York and Edoardo Mapelli Mozzi.",
                StartDate = new DateTime(2020, 7, 17),
                Location = "Royal Chapel of All Saints, Royal Lodge, Windsor",
                FamilyId = royalFamilyId,
                Type = EventType.Marriage,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Beatrice of York") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Prince William",
                Description = "The birth of Prince William, Prince of Wales.",
                StartDate = new DateTime(1982, 6, 21),
                Location = "St Mary's Hospital, London",
                FamilyId = royalFamilyId,
                Type = EventType.Birth,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "William, Prince of Wales") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of Prince Harry",
                Description = "The birth of Prince Harry, Duke of Sussex.",
                StartDate = new DateTime(1984, 9, 15),
                Location = "St Mary's Hospital, London",
                FamilyId = royalFamilyId,
                Type = EventType.Birth,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Harry, Duke of Sussex") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Birth of King Charles III",
                Description = "The birth of King Charles III.",
                StartDate = new DateTime(1948, 11, 14),
                Location = "Buckingham Palace, London",
                FamilyId = royalFamilyId,
                Type = EventType.Birth,
                RelatedMembers = new List<Member> { members.First(m => m.LastName == "Charles III") }
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Name = "Queen Elizabeth II's Coronation",
                Description = "The coronation of Queen Elizabeth II.",
                StartDate = new DateTime(1953, 6, 2),
                Location = "Westminster Abbey, London",
                FamilyId = royalFamilyId,
                Type = EventType.Other,
                RelatedMembers = new List<Member> { members.First(m => m.Id == elizabethIIId) }
            }
        });
    }
}
