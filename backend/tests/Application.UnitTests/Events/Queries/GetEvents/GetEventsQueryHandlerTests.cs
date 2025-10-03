using AutoMapper;
using backend.Application.Events.Queries.GetEvents;
using backend.Domain.Entities;
using backend.Application.Common.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Events.Queries.GetEvents;

public class GetEventsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetEventsQueryHandler _handler;
    private readonly Mock<DbSet<Event>> _mockDbSetEvent;

    public GetEventsQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockDbSetEvent = new Mock<DbSet<Event>>();

        _mockContext.Setup(c => c.Events).Returns(_mockDbSetEvent.Object);

        _handler = new GetEventsQueryHandler(_mockContext.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllEvents_WhenNoFilterIsApplied()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var events = new List<Event>
        {
            new Event { Name = "Event 1", FamilyId = familyId, Created = DateTime.UtcNow },
            new Event { Name = "Event 2", FamilyId = familyId, Created = DateTime.UtcNow }
        };
        var eventListDtos = new List<EventListDto>
        {
            new EventListDto { Name = "Event 1", FamilyId = familyId, Created = events[0].Created },
            new EventListDto { Name = "Event 2", FamilyId = familyId, Created = events[1].Created }
        };

        _mockDbSetEvent.As<IQueryable<Event>>().Setup(m => m.Provider).Returns(events.AsQueryable().Provider);
        _mockDbSetEvent.As<IQueryable<Event>>().Setup(m => m.Expression).Returns(events.AsQueryable().Expression);
        _mockDbSetEvent.As<IQueryable<Event>>().Setup(m => m.ElementType).Returns(events.AsQueryable().ElementType);
        _mockDbSetEvent.As<IQueryable<Event>>().Setup(m => m.GetEnumerator()).Returns(events.AsQueryable().GetEnumerator());

        _mockMapper.Setup(m => m.Map<IReadOnlyList<EventListDto>>(It.IsAny<IEnumerable<Event>>()))
            .Returns(eventListDtos);

        // Act
        var result = await _handler.Handle(new GetEventsQuery { FamilyId = familyId }, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new { Name = "Event 1" });
        result.Should().ContainEquivalentOf(new { Name = "Event 2" });
    }
}
