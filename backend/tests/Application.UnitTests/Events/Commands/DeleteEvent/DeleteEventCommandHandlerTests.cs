using backend.Application.Common.Exceptions;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;
using Moq; // Added for Mock
using backend.Application.Common.Interfaces; // Added for IAuthorizationService
using MediatR; // Added for IMediator

namespace backend.Application.UnitTests.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandlerTests : IDisposable
{
    private readonly DeleteEventCommandHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<IMediator> _mockMediator;

    public DeleteEventCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _mockMediator = new Mock<IMediator>();

        _handler = new DeleteEventCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldDeleteEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = new Event { Id = eventId, Name = "Test Event", FamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb") };
        _context.Events.Add(ev);
        await _context.SaveChangesAsync();

        var command = new DeleteEventCommand(eventId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedEvent = await _context.Events.FindAsync(eventId);
        deletedEvent.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var command = new DeleteEventCommand(invalidId);
        var handler = new DeleteEventCommandHandler(_context);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
