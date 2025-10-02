using backend.Application.Common.Exceptions;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandlerTests
{
    private readonly ApplicationDbContext _context;

    public UpdateEventCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldUpdateEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = new Event { Id = eventId, Name = "Test Event", FamilyId = Guid.NewGuid() };
        _context.Events.Add(ev);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            Name = "Updated Name",
        };
        var handler = new UpdateEventCommandHandler(_context);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedEvent = await _context.Events.FindAsync(eventId);
        updatedEvent.Should().NotBeNull();
        updatedEvent?.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var command = new UpdateEventCommand { Id = invalidId, Name = "test" };
        var handler = new UpdateEventCommandHandler(_context);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
