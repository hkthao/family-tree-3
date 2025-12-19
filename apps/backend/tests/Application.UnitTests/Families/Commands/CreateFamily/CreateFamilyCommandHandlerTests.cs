using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Application.FamilyMedias.DTOs;
using backend.Application.UnitTests.Common;
using backend.Domain.Common;
using backend.Domain.Entities; // NEW
using backend.Domain.Enums;
using backend.Domain.Events.Families;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CreateFamilyCommandHandler _handler;

    public CreateFamilyCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _mediatorMock = new Mock<IMediator>();
        _handler = new CreateFamilyCommandHandler(_context, _currentUserMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var avatarBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("fake image data")); // Simulate image data
        var expectedAvatarUrl = "http://uploaded.example.com/avatar.png";
        var familyMediaId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<FamilyMediaDto>.Success(new FamilyMediaDto { Id = familyMediaId, FilePath = expectedAvatarUrl }));

        // Mock the FamilyMedia DbSet to return a FamilyMedia object when FindAsync is called
        var mockFamilyMedia = new FamilyMedia { Id = familyMediaId, FilePath = expectedAvatarUrl };
        _context.FamilyMedia.Add(mockFamilyMedia);
        await _context.SaveChangesAsync();

        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            Description = "A family for testing",
            Address = "123 Test St",
            AvatarBase64 = avatarBase64,
            Visibility = "Public",
            ManagerIds = new List<Guid> { userId, Guid.NewGuid() }, // Add another manager
            ViewerIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } // Add some viewers
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
        createdFamily.AvatarUrl.Should().Be(expectedAvatarUrl); // Assert against expected uploaded URL
        createdFamily.Visibility.Should().Be(command.Visibility);
        createdFamily.Code.Should().StartWith("FAM-");
        createdFamily.FamilyUsers.Should().HaveCount(4); // Creator (Manager), 1 other Manager, 2 Viewers
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == userId && fu.Role == FamilyRole.Manager);
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == command.ManagerIds.Last() && fu.Role == FamilyRole.Manager);
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == command.ViewerIds.First() && fu.Role == FamilyRole.Viewer);
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == command.ViewerIds.Last() && fu.Role == FamilyRole.Viewer);
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is FamilyCreatedEvent) &&
            events.Any(e => e is FamilyStatsUpdatedEvent)
        )), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Verify upload was called
    }

    [Fact]
    public async Task Handle_ShouldGenerateCode_WhenCodeIsNotProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        // No AvatarBase64, so mediator should not be called
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<FamilyMediaDto>.Failure("Upload failed", "Test"));

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
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Never); // Verify upload was NOT called
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyWithOnlyAdditionalManagersAndCreator_WhenManagerIdsAreProvided()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(creatorUserId);

        var additionalManagerId = Guid.NewGuid();

        var command = new CreateFamilyCommand
        {
            Name = "Family with Managers",
            Visibility = "Private",
            ManagerIds = new List<Guid> { additionalManagerId }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdFamily = await _context.Families
                                        .Include(f => f.FamilyUsers)
                                        .FirstOrDefaultAsync(f => f.Id == result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdFamily.Should().NotBeNull();
        createdFamily!.FamilyUsers.Should().HaveCount(2); // Creator + additional Manager
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == creatorUserId && fu.Role == FamilyRole.Manager);
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == additionalManagerId && fu.Role == FamilyRole.Manager);
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyWithOnlyViewersAndCreator_WhenViewerIdsAreProvided()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(creatorUserId);

        var viewerId = Guid.NewGuid();

        var command = new CreateFamilyCommand
        {
            Name = "Family with Viewers",
            Visibility = "Private",
            ViewerIds = new List<Guid> { viewerId }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdFamily = await _context.Families
                                        .Include(f => f.FamilyUsers)
                                        .FirstOrDefaultAsync(f => f.Id == result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdFamily.Should().NotBeNull();
        createdFamily!.FamilyUsers.Should().HaveCount(2); // Creator (Manager) + Viewer
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == creatorUserId && fu.Role == FamilyRole.Manager);
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == viewerId && fu.Role == FamilyRole.Viewer);
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyWithManagersViewersAndCreator_WhenBothAreProvided()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(creatorUserId);

        var additionalManagerId = Guid.NewGuid();
        var viewerId1 = Guid.NewGuid();
        var viewerId2 = Guid.NewGuid();

        var command = new CreateFamilyCommand
        {
            Name = "Family with Managers and Viewers",
            Visibility = "Private",
            ManagerIds = new List<Guid> { additionalManagerId },
            ViewerIds = new List<Guid> { viewerId1, viewerId2 }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdFamily = await _context.Families
                                        .Include(f => f.FamilyUsers)
                                        .FirstOrDefaultAsync(f => f.Id == result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdFamily.Should().NotBeNull();
        createdFamily!.FamilyUsers.Should().HaveCount(4); // Creator (Manager) + additional Manager + 2 Viewers
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == creatorUserId && fu.Role == FamilyRole.Manager);
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == additionalManagerId && fu.Role == FamilyRole.Manager);
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == viewerId1 && fu.Role == FamilyRole.Viewer);
        createdFamily.FamilyUsers.Should().Contain(fu => fu.UserId == viewerId2 && fu.Role == FamilyRole.Viewer);
    }
}
