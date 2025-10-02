using backend.Application.Families.Commands.CreateFamily;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandlerTests
{
    private readonly CreateFamilyCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public CreateFamilyCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new CreateFamilyCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_Should_Create_Family_And_Return_Id()
    {
        // Arrange
        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            AvatarUrl = "logo.png",
            Description = "A long time ago..."
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _context.Families.Should().ContainSingle(f => f.Name == command.Name);
    }
}
