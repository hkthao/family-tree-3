using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.RecalculateFamilyStats;
using backend.Application.Families.Queries;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit; // Changed from NUnit.Framework
using backend.Application.UnitTests.Common; // NEW: Added missing using directive
using backend.Domain.Enums; // NEW: Added for Gender enum

namespace backend.Application.UnitTests.Families.Commands.RecalculateFamilyStats;

public class RecalculateFamilyStatsCommandTests : TestBase
{
    private readonly Mock<ILogger<RecalculateFamilyStatsCommandHandler>> _loggerMock;

    public RecalculateFamilyStatsCommandTests() // Changed from SetUp method to constructor
    {
        _loggerMock = new Mock<ILogger<RecalculateFamilyStatsCommandHandler>>();
    }

    [Fact] // Changed from [Test]
    public async Task Handle_GivenNonExistentFamilyId_ThrowsNotFoundException()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new RecalculateFamilyStatsCommand { FamilyId = familyId };
        var handler = new RecalculateFamilyStatsCommandHandler(_context, _loggerMock.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Entity \"Family\" ({familyId}) was not found.");
    }

    [Fact] // Changed from [Test]
    public async Task Handle_GivenExistingFamilyId_RecalculatesStatsAndSavesChanges()
    {
        // Arrange
        var family = new Family { Name = "Test Family", Description = "Description", Code = "TF001" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var originalTotalMembers = family.TotalMembers; // Should be 0 initially
        var originalTotalGenerations = family.TotalGenerations; // Should be 0 initially

        // Add some members to simulate a family tree
        var member1 = new Member("Smith", "John", "M001", family.Id, false);
        var member2 = new Member("Doe", "Jane", "M002", family.Id, false);
        var member3 = new Member("Smith", "Junior", "M003", family.Id, false);

        _context.Members.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync(CancellationToken.None);

        // For this test, we expect the domain methods to update TotalMembers and TotalGenerations correctly.
        // We ensure members are loaded for these methods to work.
        var command = new RecalculateFamilyStatsCommand { FamilyId = family.Id };
        var handler = new RecalculateFamilyStatsCommandHandler(_context, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().NotBeNull();
        
        // After recalculation, these should be updated by the Family domain methods
        result.Value!.TotalMembers.Should().Be(3); // Based on 3 members added
        result.Value.TotalGenerations.Should().BeGreaterThanOrEqualTo(1); // At least one generation, depends on relationships

        // Verify that SaveChangesAsync was called. This is implicitly tested by the data being updated.
        var updatedFamily = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == family.Id, CancellationToken.None);

        updatedFamily.Should().NotBeNull(); // FluentAssertions check
        var nonNullableUpdatedFamily = updatedFamily!; // Explicitly cast to non-nullable
        
        nonNullableUpdatedFamily.TotalMembers.Should().Be(result.Value!.TotalMembers);
        nonNullableUpdatedFamily.TotalGenerations.Should().Be(result.Value!.TotalGenerations);
    }
}

