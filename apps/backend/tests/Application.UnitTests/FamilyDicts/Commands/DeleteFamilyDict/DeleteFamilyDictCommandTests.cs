using backend.Application.Common.Exceptions;
using backend.Application.FamilyDicts.Commands.DeleteFamilyDict;
using backend.Application.UnitTests.Common;
using Moq;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.FamilyDicts.Commands.DeleteFamilyDict;

public class DeleteFamilyDictCommandTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IDateTime> _dateTimeMock;

    public DeleteFamilyDictCommandTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<IDateTime>();
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteFamilyDict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

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

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _dateTimeMock.Setup(x => x.Now).Returns(now);

        var handler = new DeleteFamilyDictCommandHandler(_context, _currentUserMock.Object, _dateTimeMock.Object);
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
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _dateTimeMock.Setup(x => x.Now).Returns(DateTime.UtcNow);

        var handler = new DeleteFamilyDictCommandHandler(_context, _currentUserMock.Object, _dateTimeMock.Object);
        var command = new DeleteFamilyDictCommand(Guid.NewGuid()); // Non-existent ID

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
