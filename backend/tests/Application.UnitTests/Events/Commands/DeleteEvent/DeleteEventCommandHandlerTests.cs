using backend.Application.Common.Exceptions;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandlerTests
{
    private readonly ApplicationDbContext _context;

    public DeleteEventCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldDeleteEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = new Event { Id = eventId, Name = "Test Event", FamilyId = Guid.NewGuid() };
        _context.Events.Add(ev);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteEventCommand(eventId);
        var handler = new DeleteEventCommandHandler(_context);

        // Act
        await handler.Handle(command, CancellationToken.None);

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
}
