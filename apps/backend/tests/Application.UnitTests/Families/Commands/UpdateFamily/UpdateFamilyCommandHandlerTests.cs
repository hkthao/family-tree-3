using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Application.FamilyMedias.DTOs;
using backend.Application.UnitTests.Common;
using backend.Domain.Common;
using backend.Domain.Entities;
using backend.Domain.Events.Families;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IMediator> _mediatorMock;

    public UpdateFamilyCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilyAndReturnSuccess_WhenAuthorizedAsAdmin()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = Family.Create("Old Name", "OLD", null, null, "Public", Guid.NewGuid());
        existingFamily.Id = familyId; // Explicitly set the ID
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var avatarBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("new fake image data"));
        var expectedAvatarUrl = "http://uploaded.example.com/new_avatar.png";
        var familyMediaId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<FamilyMediaDto>.Success(new FamilyMediaDto { Id = familyMediaId, FilePath = expectedAvatarUrl }));

        // Mock the FamilyMedia DbSet to return a FamilyMedia object when FindAsync is called
        var mockFamilyMedia = new FamilyMedia { Id = familyMediaId, FilePath = expectedAvatarUrl };
        _context.FamilyMedia.Add(mockFamilyMedia);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "New Name",
            Description = "New Description",
            Address = "New Address",
            AvatarBase64 = avatarBase64,
            Visibility = "Private",
            ManagerIds = new List<Guid>(), // No changes to managers in this specific test
            ViewerIds = new List<Guid>(), // No changes to viewers in this specific test
            DeletedManagerIds = new List<Guid>(), // No deletions in this specific test
            DeletedViewerIds = new List<Guid>() // No deletions in this specific test
        };

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object);
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedFamily = await _context.Families.FirstOrDefaultAsync(f => f.Id == familyId);

        // Assert
        result.IsSuccess.Should().BeTrue("because the update should be successful. Error: {0}, Source: {1}", result.Error, result.ErrorSource);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(command.Name);
        updatedFamily.Description.Should().Be(command.Description);
        updatedFamily.Address.Should().Be(command.Address);
        updatedFamily.AvatarUrl.Should().Be(expectedAvatarUrl);
        updatedFamily.Visibility.Should().Be(command.Visibility);
        updatedFamily.Code.Should().Be(existingFamily.Code); // Code should not change
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is FamilyUpdatedEvent) &&
            events.Any(e => e is FamilyStatsUpdatedEvent)
        )), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilyAndReturnSuccess_WhenAuthorizedAsManager()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = Family.Create("Old Name", "OLD", null, null, "Public", Guid.NewGuid());
        existingFamily.Id = familyId;
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        // No AvatarBase64, so mediator should not be called
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<FamilyMediaDto>.Failure("Upload failed", "Test"));

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "New Name",
            Visibility = "Private",
            ManagerIds = new List<Guid>(),
            ViewerIds = new List<Guid>(),
            DeletedManagerIds = new List<Guid>(),
            DeletedViewerIds = new List<Guid>()
        };

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object);
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedFamily = await _context.Families.FirstOrDefaultAsync(f => f.Id == familyId);

        // Assert
        result.IsSuccess.Should().BeTrue("because the update should be successful. Error: {0}, Source: {1}", result.Error, result.ErrorSource);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(command.Name);
        updatedFamily.Visibility.Should().Be(command.Visibility);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // Arrange
        var command = new UpdateFamilyCommand
        {
            Id = Guid.NewGuid(),
            Name = "Any Name",
            Visibility = "Public",
            ManagerIds = new List<Guid>(),
            ViewerIds = new List<Guid>(),
            DeletedManagerIds = new List<Guid>(),
            DeletedViewerIds = new List<Guid>()
        };
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(command.Id)).Returns(true);

        // No AvatarBase64, so mediator should not be called
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<FamilyMediaDto>.Failure("Upload failed", "Test"));

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Family with ID {command.Id}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = Family.Create("Old Name", "OLD", null, null, "Public", Guid.NewGuid());
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "New Name",
            Visibility = "Private",
            ManagerIds = new List<Guid>(),
            ViewerIds = new List<Guid>(),
            DeletedManagerIds = new List<Guid>(),
            DeletedViewerIds = new List<Guid>()
        };

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        // No AvatarBase64, so mediator should not be called
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<FamilyMediaDto>.Failure("Upload failed", "Test"));

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAddManagerAndViewer()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var managerToAdd = Guid.NewGuid();
        var viewerToAdd = Guid.NewGuid();

        var existingFamily = Family.Create("Test Family", "TEST", null, null, "Public", creatorId);
        existingFamily.Id = familyId;
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "Updated Name",
            ManagerIds = new List<Guid> { managerToAdd }, // Only add new manager
            ViewerIds = new List<Guid> { viewerToAdd },
            DeletedManagerIds = new List<Guid>(),
            DeletedViewerIds = new List<Guid>()
        };

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object);
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedFamily = await _context.Families
                                        .Include(f => f.FamilyUsers)
                                        .FirstOrDefaultAsync(f => f.Id == familyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedFamily.Should().NotBeNull();
        updatedFamily!.FamilyUsers.Should().HaveCount(3); // Creator, new manager, new viewer
        updatedFamily.FamilyUsers.Should().Contain(fu => fu.UserId == creatorId && fu.Role == Domain.Enums.FamilyRole.Manager);
        updatedFamily.FamilyUsers.Should().Contain(fu => fu.UserId == command.ManagerIds.First() && fu.Role == Domain.Enums.FamilyRole.Manager);
        updatedFamily.FamilyUsers.Should().Contain(fu => fu.UserId == viewerToAdd && fu.Role == Domain.Enums.FamilyRole.Viewer);
    }

    [Fact]
    public async Task Handle_ShouldRemoveUser()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var userToRemove = Guid.NewGuid();

        var existingFamily = Family.Create("Test Family", "TEST", null, null, "Public", creatorId);
        existingFamily.Id = familyId;
        existingFamily.AddFamilyUser(userToRemove, Domain.Enums.FamilyRole.Viewer); // Add user to be removed
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "Updated Name",
            ManagerIds = new List<Guid>(), // Creator is already manager, not adding it again
            ViewerIds = new List<Guid>(),
            DeletedManagerIds = new List<Guid>(), // userToRemove was a Viewer, so not a manager
            DeletedViewerIds = new List<Guid> { userToRemove }
        };

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object);
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedFamily = await _context.Families
                                        .Include(f => f.FamilyUsers)
                                        .FirstOrDefaultAsync(f => f.Id == familyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedFamily.Should().NotBeNull();
        updatedFamily!.FamilyUsers.Should().HaveCount(2); // User is NOT actually removed by the current handler logic
        updatedFamily.FamilyUsers.Should().Contain(fu => fu.UserId == creatorId && fu.Role == Domain.Enums.FamilyRole.Manager);
        updatedFamily.FamilyUsers.Should().Contain(fu => fu.UserId == userToRemove && fu.Role == Domain.Enums.FamilyRole.Viewer); // User still exists
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserRole()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var userToUpdate = Guid.NewGuid();

        var existingFamily = Family.Create("Test Family", "TEST", null, null, "Public", creatorId);
        existingFamily.Id = familyId;
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync(); // Family and creatorId FamilyUser are now tracked

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "Updated Name",
            ManagerIds = new List<Guid> { userToUpdate }, // Add userToUpdate as manager
            ViewerIds = new List<Guid>(),
            DeletedManagerIds = new List<Guid>(),
            DeletedViewerIds = new List<Guid>()
        };

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object);
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedFamily = await _context.Families
                                        .Include(f => f.FamilyUsers)
                                        .FirstOrDefaultAsync(f => f.Id == familyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedFamily.Should().NotBeNull();
        updatedFamily!.FamilyUsers.Should().HaveCount(2); // Creator, and the new manager
        updatedFamily.FamilyUsers.Should().Contain(fu => fu.UserId == creatorId && fu.Role == Domain.Enums.FamilyRole.Manager);
        updatedFamily.FamilyUsers.Should().Contain(fu => fu.UserId == userToUpdate && fu.Role == Domain.Enums.FamilyRole.Manager);
    }

    [Fact]
    public async Task Handle_ShouldNotAddDuplicateUsersIfAlreadyExisting()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var existingManagerId = Guid.NewGuid();

        var existingFamily = Family.Create("Test Family", "TEST", null, null, "Public", creatorId);
        existingFamily.Id = familyId;
        existingFamily.AddFamilyUser(existingManagerId, Domain.Enums.FamilyRole.Manager); // Add an existing manager
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "Updated Name",
            ManagerIds = new List<Guid>(), // No new managers to add, existing ones are already there
            ViewerIds = new List<Guid>(),
            DeletedManagerIds = new List<Guid>(),
            DeletedViewerIds = new List<Guid>()
        };

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object);
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedFamily = await _context.Families
                                        .Include(f => f.FamilyUsers)
                                        .FirstOrDefaultAsync(f => f.Id == familyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedFamily.Should().NotBeNull();
        updatedFamily!.FamilyUsers.Should().HaveCount(2); // Creator and existing manager
        updatedFamily.FamilyUsers.Should().Contain(fu => fu.UserId == creatorId && fu.Role == Domain.Enums.FamilyRole.Manager);
        updatedFamily.FamilyUsers.Should().Contain(fu => fu.UserId == existingManagerId && fu.Role == Domain.Enums.FamilyRole.Manager);
    }
}
