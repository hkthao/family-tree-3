using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using backend.Domain.Events.Families;
using backend.Domain.Common; // NEW
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly CreateFamilyCommandHandler _handler;

    public CreateFamilyCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new CreateFamilyCommandHandler(_context, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            Description = "A family for testing",
            Address = "123 Test St",
            AvatarUrl = "http://example.com/avatar.png",
            Visibility = "Public"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdFamily = await _context.Families
                                        .Include(f => f.FamilyUsers)
                                        .FirstOrDefaultAsync(f => f.Id == result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        createdFamily.Should().NotBeNull();
        createdFamily!.Name.Should().Be(command.Name);
        createdFamily.Description.Should().Be(command.Description);
        createdFamily.Address.Should().Be(command.Address);
        createdFamily.AvatarUrl.Should().Be(command.AvatarUrl);
        createdFamily.Visibility.Should().Be(command.Visibility);
        createdFamily.Code.Should().StartWith("FAM-");
        createdFamily.FamilyUsers.Should().ContainSingle(fu => fu.UserId == userId && fu.Role == FamilyRole.Manager);
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is FamilyCreatedEvent) &&
            events.Any(e => e is FamilyStatsUpdatedEvent)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGenerateCode_WhenCodeIsNotProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var command = new CreateFamilyCommand
        {
            Name = "Family without code",
            Visibility = "Private"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdFamily = await _context.Families.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdFamily.Should().NotBeNull();
        createdFamily!.Code.Should().NotBeNullOrEmpty();
        createdFamily.Code.Should().StartWith("FAM-");
    }
}
