using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.FamilyDicts.Commands.DeleteFamilyDict;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyDicts;

public class DeleteFamilyDictCommandTests : TestBase
{
    public DeleteFamilyDictCommandTests() : base()
    {
    }

    [Fact]
    public async Task Handle_ShouldDeleteFamilyDict()
    {
        // Arrange
        var initialFamilyDict = new FamilyDict
        {
            Id = Guid.NewGuid(),
            Name = "FamilyDict to Delete",
            Type = FamilyDictType.Blood,
            Description = "Description",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "N", Central = "C", South = "S" }
        };
        _context.FamilyDicts.Add(initialFamilyDict);
        await _context.SaveChangesAsync();

        var handler = new DeleteFamilyDictCommandHandler(_context);
        var command = new DeleteFamilyDictCommand(initialFamilyDict.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedFamilyDict = await _context.FamilyDicts.FindAsync(initialFamilyDict.Id);
        deletedFamilyDict.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenFamilyDictDoesNotExist()
    {
        // Arrange
        var handler = new DeleteFamilyDictCommandHandler(_context);
        var command = new DeleteFamilyDictCommand(Guid.NewGuid()); // Non-existent ID

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
