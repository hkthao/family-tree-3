using backend.Application.AI.DTOs; // NEW
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // NEW
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.Files.DTOs;
using backend.Application.Files.UploadFile; // NEW
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using backend.Domain.Entities;
using backend.Domain.Events.Families;
using FluentAssertions;
using MediatR; // NEW
using Microsoft.EntityFrameworkCore; // NEW
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IMediator> _mediatorMock; // NEW

    public UpdateFamilyCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _mediatorMock = new Mock<IMediator>(); // NEW
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilyAndReturnSuccess_WhenAuthorizedAsAdmin()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = Family.Create("Old Name", "OLD", null, null, "Public", Guid.NewGuid());
        existingFamily.Id = familyId; // Explicitly set the ID
        // existingFamily.UpdateAvatar("http://old.avatar.com"); // Removed to avoid double event dispatch
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var avatarBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("new fake image data"));
        var expectedAvatarUrl = "http://uploaded.example.com/new_avatar.png";

        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = expectedAvatarUrl }));

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "New Name",
            Description = "New Description",
            Address = "New Address",
            AvatarBase64 = avatarBase64, // Use AvatarBase64
            Visibility = "Private",
            Code = "NEW"
        };

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object); // Pass mediator mock
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedFamily = await _context.Families.FirstOrDefaultAsync(f => f.Id == familyId);

        // Assert
        result.IsSuccess.Should().BeTrue("because the update should be successful. Error: {0}, Source: {1}", result.Error, result.ErrorSource);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(command.Name);
        updatedFamily.Description.Should().Be(command.Description);
        updatedFamily.Address.Should().Be(command.Address);
        updatedFamily.AvatarUrl.Should().Be(expectedAvatarUrl); // Assert against expected uploaded URL
        updatedFamily.Visibility.Should().Be(command.Visibility);
        updatedFamily.Code.Should().Be(command.Code);
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is FamilyUpdatedEvent) && // Only one FamilyUpdatedEvent from handler
            events.Any(e => e is FamilyStatsUpdatedEvent)
        )), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Verify upload was called
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilyAndReturnSuccess_WhenAuthorizedAsManager()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = Family.Create("Old Name", "OLD", null, null, "Public", Guid.NewGuid());
        existingFamily.Id = familyId; // Explicitly set the ID
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        // No AvatarBase64, so mediator should not be called
        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<ImageUploadResponseDto>.Failure("Upload failed", "Test"));

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "New Name",
            Visibility = "Private"
        };

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object); // Pass mediator mock
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedFamily = await _context.Families.FirstOrDefaultAsync(f => f.Id == familyId);

        // Assert
        result.IsSuccess.Should().BeTrue("because the update should be successful. Error: {0}, Source: {1}", result.Error, result.ErrorSource);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(command.Name);
        updatedFamily.Visibility.Should().Be(command.Visibility);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Never); // Verify upload was NOT called
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Any Name", Visibility = "Public" };
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(command.Id)).Returns(true);

        // No AvatarBase64, so mediator should not be called
        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<ImageUploadResponseDto>.Failure("Upload failed", "Test"));

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object); // Pass mediator mock
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, command.Id));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Never); // Verify upload was NOT called
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
        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<ImageUploadResponseDto>.Failure("Upload failed", "Test"));

        // Act
        var _handler = new UpdateFamilyCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object); // Pass mediator mock
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Never); // Verify upload was NOT called
    }
}
