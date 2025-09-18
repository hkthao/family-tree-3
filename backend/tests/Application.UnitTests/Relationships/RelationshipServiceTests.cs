using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.Relationships;

public class RelationshipServiceTests
{
    private readonly ApplicationDbContext _context;

    public RelationshipServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateRelationship_ShouldCreateRelationship_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Parent,
            FamilyId = Guid.NewGuid(),
            StartDate = DateTime.Now,
            EndDate = DateTime.Now
        };
        var handler = new CreateRelationshipCommandHandler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdRelationship = await _context.Relationships.FindAsync(result);
        createdRelationship.Should().NotBeNull();
        createdRelationship?.SourceMemberId.Should().Be(command.SourceMemberId);
        createdRelationship?.TargetMemberId.Should().Be(command.TargetMemberId);
    }

    [Fact]
    public async Task CreateRelationship_ShouldThrowValidationException_WhenMemberIdAndTargetIdAreSame()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = memberId,
            TargetMemberId = memberId,
            Type = RelationshipType.Parent,
            FamilyId = Guid.NewGuid(),
            StartDate = DateTime.Now,
            EndDate = DateTime.Now
        };
        var validator = new CreateRelationshipCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
