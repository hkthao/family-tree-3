using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Domain.Enums;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandHandlerTests
{
    private readonly CreateRelationshipCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public CreateRelationshipCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new CreateRelationshipCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldCreateRelationshipAndReturnId()
    {
        // Arrange
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = Guid.NewGuid(),
            Type = RelationshipType.Parent,
            TargetMemberId = Guid.NewGuid(),
            FamilyId = Guid.NewGuid(),
            StartDate = new DateTime(2000, 1, 1)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _context.Relationships.Should().ContainSingle(r => r.Id == result);
    }
}
