using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public UpdateFamilyCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamily_WhenValidCommandAndUserIsAdmin()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Old Family Name",
            Code = "OLD001",
            Description = "Old description",
            Address = "Old Address",
            Visibility = "Private"
        };

        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "New Family Name",
            Code = "NEW001",
            Description = "New description",
            Address = "New Address",
            Visibility = "Public"
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new UpdateFamilyCommandHandler(handlerContext, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedFamily = await handlerContext.Families.FindAsync(familyId);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(command.Name);
        updatedFamily.Code.Should().Be(command.Code);
        updatedFamily.Description.Should().Be(command.Description);
        updatedFamily.Address.Should().Be(command.Address);
        updatedFamily.Visibility.Should().Be(command.Visibility);
    }
}
