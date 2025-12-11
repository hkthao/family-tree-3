using backend.Application.FamilyDicts.Commands.DeleteFamilyDict;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.FamilyDicts.Commands.DeleteFamilyDict;

public class DeleteFamilyDictCommandTests : TestBase
{
    public DeleteFamilyDictCommandTests()
    {
        // Set up authenticated user by default for most tests
        _mockUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteFamilyDict()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.FamilyDicts.RemoveRange(_context.FamilyDicts);
        await _context.SaveChangesAsync();

        var userId = _mockUser.Object.UserId; // Use mocked user ID
        var now = _mockDateTime.Object.Now; // Use mocked DateTime

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

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        var handler = new DeleteFamilyDictCommandHandler(_context, _mockUser.Object, _mockDateTime.Object, _mockAuthorizationService.Object);
        var command = new DeleteFamilyDictCommand(initialFamilyDict.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedFamilyDict = await _context.FamilyDicts.FindAsync(initialFamilyDict.Id);
        deletedFamilyDict.Should().NotBeNull();
        deletedFamilyDict!.IsDeleted.Should().BeTrue();
        deletedFamilyDict.DeletedBy.Should().Be(userId.ToString());
        deletedFamilyDict.DeletedDate.Should().Be(now);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenFamilyDictDoesNotExist()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.FamilyDicts.RemoveRange(_context.FamilyDicts);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        var handler = new DeleteFamilyDictCommandHandler(_context, _mockUser.Object, _mockDateTime.Object, _mockAuthorizationService.Object);
        var command = new DeleteFamilyDictCommand(Guid.NewGuid()); // Non-existent ID

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsNotAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Simulate non-admin user
        var handler = new DeleteFamilyDictCommandHandler(_context, _mockUser.Object, _mockDateTime.Object, _mockAuthorizationService.Object);
        var command = new DeleteFamilyDictCommand(Guid.NewGuid()); // ID does not matter for this test

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }
}
