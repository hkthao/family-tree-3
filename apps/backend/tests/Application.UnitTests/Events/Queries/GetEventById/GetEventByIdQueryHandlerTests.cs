
using AutoMapper;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetEventById;

public class GetEventByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetEventByIdQueryHandler _handler;

    public GetEventByIdQueryHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _handler = new GetEventByIdQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEvent_WhenEventExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var testEvent = new Event("Test Event", "EVT-TEST", EventType.Other, Guid.NewGuid()) { Id = eventId };
        _context.Events.Add(testEvent);
        await _context.SaveChangesAsync();

        var eventDetailDto = new EventDetailDto { Id = eventId, Name = "Test Event" };

        var query = new GetEventByIdQuery(eventId);

        // This setup is more complex because ProjectTo is an extension method.
        // A simpler way for unit tests is to mock the final result of the projection.
        // However, for this test, we will rely on the in-memory provider's ability to execute the query.
        // We need a real AutoMapper configuration for ProjectTo to work.
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Event, EventDetailDto>();
        });
        var mapper = configuration.CreateMapper();
        var handlerWithRealMapper = new GetEventByIdQueryHandler(_context, mapper);


        // Act
        var result = await handlerWithRealMapper.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(eventId);
        result.Value.Name.Should().Be(testEvent.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEventDoesNotExist()
    {
        // Arrange
        var query = new GetEventByIdQuery(Guid.NewGuid());
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Event, EventDetailDto>();
        });
        var mapper = configuration.CreateMapper();
        var handlerWithRealMapper = new GetEventByIdQueryHandler(_context, mapper);

        // Act
        var result = await handlerWithRealMapper.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
