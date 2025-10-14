using AutoMapper;
using backend.Application.Events.Commands.CreateEvent;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;
using backend.Application.Identity.UserProfiles.Queries; // Added for MappingProfile
using Moq; // Added for Mock
using backend.Application.Common.Interfaces; // Added for IAuthorizationService
using MediatR; // Added for IMediator

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests : IDisposable
{
    private readonly CreateEventCommandHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<IMediator> _mockMediator;

    public CreateEventCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _mockMediator = new Mock<IMediator>();

        _handler = new CreateEventCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockMediator.Object);
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