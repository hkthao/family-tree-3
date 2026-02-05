using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces.Family;
using backend.Application.Families.Commands.RecalculateFamilyStats;
using backend.Application.UnitTests.Common; // NEW: Added missing using directive
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit; // Changed from NUnit.Framework

namespace backend.Application.UnitTests.Families.Commands.RecalculateFamilyStats;

public class RecalculateFamilyStatsCommandTests : TestBase
{
    private readonly Mock<ILogger<RecalculateFamilyStatsCommandHandler>> _loggerMock;
    private readonly Mock<IFamilyTreeService> _familyTreeServiceMock; // NEW: Mock IFamilyTreeService

    public RecalculateFamilyStatsCommandTests() // Changed from SetUp method to constructor
    {
        _loggerMock = new Mock<ILogger<RecalculateFamilyStatsCommandHandler>>();
        _familyTreeServiceMock = new Mock<IFamilyTreeService>(); // NEW: Initialize mock
    }

    [Fact] // Changed from [Test]
    public async Task Handle_GivenNonExistentFamilyId_ThrowsNotFoundException()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new RecalculateFamilyStatsCommand { FamilyId = familyId };
        var handler = new RecalculateFamilyStatsCommandHandler(_context, _loggerMock.Object, _familyTreeServiceMock.Object); // NEW: Pass mock

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

        // Configure mock service to update family stats
        _familyTreeServiceMock.Setup(s => s.UpdateFamilyStats(family.Id, CancellationToken.None))
            .Callback<Guid, CancellationToken>(async (fId, ct) =>
            {
                // Simulate service updating the family stats on the actual family entity in the context
                var f = await _context.Families.FindAsync(fId);
                if (f != null)
                {
                    f.TotalMembers = 3; // Hardcode expected value for test
                    f.TotalGenerations = 2; // Hardcode expected value for test
                    await _context.SaveChangesAsync(ct); // Save changes to reflect in the context
                }
            })
            .Returns(Task.CompletedTask);

        var command = new RecalculateFamilyStatsCommand { FamilyId = family.Id };
        var handler = new RecalculateFamilyStatsCommandHandler(_context, _loggerMock.Object, _familyTreeServiceMock.Object); // NEW: Pass mock

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().NotBeNull();

        result.Value!.TotalMembers.Should().Be(3);
        result.Value.TotalGenerations.Should().Be(2);

        // Verify that SaveChangesAsync was called. This is implicitly tested by the data being updated.
        var updatedFamily = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == family.Id, CancellationToken.None);

        updatedFamily.Should().NotBeNull();
        updatedFamily!.TotalMembers.Should().Be(3);
        updatedFamily.TotalGenerations.Should().Be(2);

        // Verify that the service method was called
        _familyTreeServiceMock.Verify(s => s.UpdateFamilyStats(family.Id, CancellationToken.None), Times.Once);
    }
}

