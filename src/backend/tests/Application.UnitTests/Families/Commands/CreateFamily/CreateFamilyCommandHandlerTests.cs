using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public CreateFamilyCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    [Fact]
    public async Task Handle_ShouldCreateFamily_WhenValidCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            Code = "TF001",
            Description = "A test family",
            Address = "123 Test St",
            Visibility = "Private"
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new CreateFamilyCommandHandler(handlerContext, _currentUserMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var family = await handlerContext.Families.FindAsync(result.Value);
        family.Should().NotBeNull();
        family!.Name.Should().Be(command.Name);
        family.Code.Should().Be(command.Code);
        family.Description.Should().Be(command.Description);
        family.Address.Should().Be(command.Address);
        family.Visibility.Should().Be(command.Visibility);

        var familyUser = await handlerContext.FamilyUsers.FirstOrDefaultAsync(fu => fu.FamilyId == family.Id && fu.UserId == userId);
        familyUser.Should().NotBeNull();
        familyUser!.Role.Should().Be(Domain.Enums.FamilyRole.Manager);
    }
}
