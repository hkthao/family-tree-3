using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Commands.DeleteRelationship;
using backend.Domain.Entities;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using backend.Infrastructure.Data;

namespace backend.Application.UnitTests.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandlerTests
{
    private readonly DeleteRelationshipCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public DeleteRelationshipCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new DeleteRelationshipCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldDeleteRelationship_WhenRelationshipExists()
    {
        // Arrange
        var relationshipId = "65e6f8a2b3c4d5e6f7a8b9c0";
        var relationship = new Relationship { Id = relationshipId };
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        var command = new DeleteRelationshipCommand(relationshipId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _context.Relationships.Should().NotContain(r => r.Id == relationshipId);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var command = new DeleteRelationshipCommand("000000000000000000000000"); // Valid format, but non-existent

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}