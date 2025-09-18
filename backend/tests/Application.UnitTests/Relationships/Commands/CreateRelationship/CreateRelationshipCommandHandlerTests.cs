using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using backend.Infrastructure.Data;

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
            MemberId = "65e6f8a2b3c4d5e6f7a8b9c0",
            Type = RelationshipType.Parent,
            TargetId = "65e6f8a2b3c4d5e6f7a8b9c1",
            FamilyId = "65e6f8a2b3c4d5e6f7a8b9c2",
            StartDate = new DateTime(2000, 1, 1)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNullOrEmpty();
        _context.Relationships.Should().ContainSingle(r => r.Id == result);
    }
}