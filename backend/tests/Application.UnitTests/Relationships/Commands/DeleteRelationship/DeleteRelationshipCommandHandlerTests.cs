using backend.Application.Common.Exceptions;
using backend.Application.Relationships.Commands.DeleteRelationship;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly DeleteRelationshipCommandHandler _handler;

    public DeleteRelationshipCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new DeleteRelationshipCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldDeleteRelationship_WhenRelationshipExists()
    {
        // Arrange
        var relationshipId = Guid.NewGuid();
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
        var command = new DeleteRelationshipCommand(Guid.Empty);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}