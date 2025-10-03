using AutoMapper;
using backend.Application.Events.Commands.CreateEvent;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Application.Common.Mappings;
using backend.Infrastructure.Data;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests : IDisposable
{
    private readonly CreateEventCommandHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateEventCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new CreateEventCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_Should_Create_Event()
    {
        // Arrange
        var command = new CreateEventCommand
        {
            Name = "New Event",
            Description = "A new event",
        };

        // Act
        var eventId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdEvent = await _context.Events.FindAsync(eventId);
        createdEvent.Should().NotBeNull();
        createdEvent!.Name.Should().Be("New Event");
        createdEvent.Description.Should().Be("A new event");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}