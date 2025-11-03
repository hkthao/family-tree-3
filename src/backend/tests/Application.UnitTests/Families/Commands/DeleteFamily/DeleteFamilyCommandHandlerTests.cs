using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public DeleteFamilyCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    [Fact]
    public async Task Handle_ShouldDeleteFamily_WhenValidCommandAndUserIsAdmin()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);
        _mockDateTime.Setup(x => x.Now).Returns(now);

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Family to Delete",
            Code = "DEL001",
            Description = "Description",
            Address = "Address",
            Visibility = "Private"
        };

        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        var command = new DeleteFamilyCommand(familyId);

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new DeleteFamilyCommandHandler(handlerContext, _authorizationServiceMock.Object, _currentUserMock.Object, _mockDateTime.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedFamily = await handlerContext.Families.FindAsync(familyId);
        deletedFamily.Should().NotBeNull(); // Family should still exist but be marked as deleted
        deletedFamily!.IsDeleted.Should().BeTrue();
        deletedFamily.DeletedBy.Should().Be(userId.ToString());
        deletedFamily.DeletedDate.Should().Be(now);
    }
}
