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
            Code = "NEW"
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
            Visibility = "Private"
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
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Any Name", Visibility = "Public" };
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
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, command.Id));
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

        var command = new UpdateFamilyCommand { Id = familyId, Name = "New Name", Visibility = "Private" };

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
}
