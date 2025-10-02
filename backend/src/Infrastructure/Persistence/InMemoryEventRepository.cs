using backend.Domain.Entities;
using backend.Application.Common.Interfaces;
using backend.Domain.Enums;

namespace backend.Infrastructure.Persistence;

public class InMemoryEventRepository : InMemoryRepository<Event>, IEventRepository
{
    public InMemoryEventRepository()
    {
        Guid royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb");

        // Get member Guids from InMemoryMemberRepository
        var memberRepo = new InMemoryMemberRepository();
        var williamId = memberRepo._items.First(m => m.FirstName == "Prince" && m.LastName == "William, Prince of Wales").Id;
        var catherineId = memberRepo._items.First(m => m.FirstName == "Catherine," && m.LastName == "Princess of Wales").Id;
        var georgeId = memberRepo._items.First(m => m.FirstName == "Prince" && m.LastName == "George of Wales").Id;
        var elizabethIIId = memberRepo._items.First(m => m.FirstName == "Queen" && m.LastName == "Elizabeth II").Id;

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
                RelatedMembers = new List<Member> { memberRepo._items.First(m => m.Id == georgeId) }
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
                RelatedMembers = new List<Member> { memberRepo._items.First(m => m.Id == williamId), memberRepo._items.First(m => m.Id == catherineId) }
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
                RelatedMembers = new List<Member> { memberRepo._items.First(m => m.Id == elizabethIIId) }
            }
        });
    }
}
