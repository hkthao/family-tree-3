
using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Application.Common.Exceptions;
using backend.Application.Relationships.Commands.UpdateRelationship;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandHandlerTests
{
    private readonly UpdateRelationshipCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public UpdateRelationshipCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new UpdateRelationshipCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldUpdateRelationship_WhenRelationshipExists()
    {
        // Arrange
        var relationshipId = Guid.NewGuid();
        var existingRelationship = new Relationship
        {
            Id = relationshipId,
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Parent,
            FamilyId = Guid.NewGuid(),
            StartDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        _context.Relationships.Add(existingRelationship);
        await _context.SaveChangesAsync();

        var command = new UpdateRelationshipCommand
        {
            Id = relationshipId,
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Child,
            FamilyId = Guid.NewGuid(),
            StartDate = new DateTime(2005, 5, 5, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedRelationship = await _context.Relationships.FindAsync(relationshipId);
        updatedRelationship.Should().NotBeNull();
        updatedRelationship!.SourceMemberId.Should().Be(command.SourceMemberId);
        updatedRelationship.TargetMemberId.Should().Be(command.TargetMemberId);
        updatedRelationship.Type.Should().Be(command.Type);
        updatedRelationship.FamilyId.Should().Be(command.FamilyId);
        updatedRelationship.StartDate.Should().Be(command.StartDate);
        updatedRelationship.EndDate.Should().Be(command.EndDate);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var relationshipId = Guid.NewGuid();
        var command = new UpdateRelationshipCommand
        {
            Id = relationshipId,
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Child,
            FamilyId = Guid.NewGuid(),
            StartDate = new DateTime(2005, 5, 5),
            EndDate = new DateTime(2020, 10, 10)
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
