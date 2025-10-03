using AutoMapper;
using backend.Application.Events.Queries.GetEvents;
using backend.Domain.Entities;
using backend.Application.Common.Interfaces;
using backend.Application.Events;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Mappings;

namespace backend.Application.UnitTests.Events.Queries.GetEvents;

public class GetEventsQueryHandlerTests
{
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly IMapper _mapper;
    private readonly GetEventsQueryHandler _handler;

    public GetEventsQueryHandlerTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configurationProvider.CreateMapper();
        _handler = new GetEventsQueryHandler(_mockEventRepository.Object,  _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllEvents_WhenNoFilterIsApplied()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var events = new List<Event>
        {
            new Event { Name = "Event 1", FamilyId = familyId },
            new Event { Name = "Event 2", FamilyId = familyId }
        };
        var eventDtos = new List<EventDto>
        {
            new EventDto { Name = "Event 1", FamilyId = familyId },
            new EventDto { Name = "Event 2", FamilyId = familyId }
        };

        _mockEventRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(events);
        // Act
        var result = await _handler.Handle(new GetEventsQuery { FamilyId = familyId }, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }
}
