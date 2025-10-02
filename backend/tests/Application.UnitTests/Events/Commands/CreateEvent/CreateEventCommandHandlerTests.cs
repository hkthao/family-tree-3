using backend.Application.Events.Commands.CreateEvent;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests
{
    private readonly ApplicationDbContext _context;

    public CreateEventCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldCreateEventAndReturnId()
    {
        // Arrange
        var command = new CreateEventCommand
        {
            Name = "Test Event",
            StartDate = new DateTime(2023, 1, 1),
            FamilyId = Guid.NewGuid()
        };

        var handler = new CreateEventCommandHandler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdEvent = await _context.Events.FindAsync(result);
        createdEvent.Should().NotBeNull();
        createdEvent!.Name.Should().Be("Test Event");
    }
}
