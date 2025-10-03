using backend.Application.Events.Commands.CreateEvent;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests : IDisposable
{
    private readonly CreateEventCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public CreateEventCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new CreateEventCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldCreateEventAndReturnId()
    {
        // Arrange
        var command = new CreateEventCommand
        {
            Name = "New Test Event",
            StartDate = new DateTime(2023, 1, 1),
            FamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"), // Use a seeded family ID
            RelatedMembers = new List<Guid>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        var createdEvent = await _context.Events.FindAsync(result);
        createdEvent.Should().NotBeNull();
        createdEvent!.Name.Should().Be(command.Name);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
