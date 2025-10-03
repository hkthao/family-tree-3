using backend.Application.Common.Exceptions;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;

namespace backend.Application.UnitTests.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandlerTests : IDisposable
{
    private readonly UpdateEventCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public UpdateEventCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new UpdateEventCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldUpdateEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var familyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Use a seeded family ID
        var memberId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"); // Use a seeded member ID

        var ev = new Event { Id = eventId, Name = "Test Event", FamilyId = familyId, RelatedMembers = new List<Member>() };
        _context.Events.Add(ev);
        await _context.SaveChangesAsync();

        var command = new UpdateEventCommand
        {
            Id = eventId,
            Name = "Updated Name",
            RelatedMembers = new List<Guid> { memberId }
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedEvent = await _context.Events.Include(e => e.RelatedMembers).FirstOrDefaultAsync(e => e.Id == eventId);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Name.Should().Be(command.Name);
        updatedEvent.RelatedMembers.Should().ContainSingle(m => m.Id == memberId);
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "test" };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
