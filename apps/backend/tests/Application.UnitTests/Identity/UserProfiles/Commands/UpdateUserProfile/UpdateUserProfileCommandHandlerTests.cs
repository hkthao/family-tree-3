using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.DTOs;
using backend.Application.Files.UploadFile;
using backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandlerTests : TestBase
{
    private readonly UpdateUserProfileCommandHandler _handler;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ICurrentUser> _mockCurrentUser;

    public UpdateUserProfileCommandHandlerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _handler = new UpdateUserProfileCommandHandler(_context, _mockMediator.Object, _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand();
        command.SetId(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.UserProfileNotFound);
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserProfile_WhenValidCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("auth0|123", "old@example.com", "Test User", "Test", "User", null, null);
        user.Id = userId; // Set the ID after construction
        user.Profile!.Update(
            "external-id",
            "old@example.com",
            "Old Name",
            "Old",
            "Name",
            "111",
            "old_avatar.png"
        );
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync(CancellationToken.None);

        var newAvatarBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="; // A tiny valid base64 PNG
        var uploadedAvatarUrl = "http://example.com/uploaded_avatar.png";

        var command = new UpdateUserProfileCommand
        {
            Email = "new@example.com",
            FirstName = "New",
            LastName = "User",
            Phone = "222",
            AvatarBase64 = newAvatarBase64
        };
        command.SetId(user.Profile.Id);

        _mockMediator.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = uploadedAvatarUrl }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Profile.Email.Should().Be(command.Email);
        user.Profile.Name.Should().Be("New User");
        user.Profile.FirstName.Should().Be(command.FirstName);
        user.Profile.LastName.Should().Be(command.LastName);
        user.Profile.Phone.Should().Be(command.Phone);
        user.Profile.Avatar.Should().Be(uploadedAvatarUrl); // Assert the uploaded URL
        _mockMediator.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Verify upload was called
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserProfileWithPartialData_WhenSomeFieldsAreNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("auth0|123", "old@example.com");
        user.Id = userId; // Set the ID after construction
        user.Profile!.Update(
            "external-id",
            "old@example.com",
            "Old Name",
            "Old",
            "Name",
            "111",
            "old_avatar.png"
        );
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync(CancellationToken.None);

        var newAvatarBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="; // A tiny valid base64 PNG
        var uploadedAvatarUrl = "http://example.com/uploaded_avatar_partial.png";

        var command = new UpdateUserProfileCommand
        {
            Email = "new@example.com",
            FirstName = null, // Should retain old value
            LastName = "Updated",
            Phone = null, // Should retain old value
            AvatarBase64 = newAvatarBase64
        };

        command.SetId(user.Profile.Id);

        _mockMediator.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = uploadedAvatarUrl }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Profile.Email.Should().Be(command.Email);
        user.Profile.Name.Should().Be("Old Updated"); // FirstName is null, so it uses old FirstName + new LastName
        user.Profile.FirstName.Should().Be("Old");
        user.Profile.LastName.Should().Be(command.LastName);
        user.Profile.Phone.Should().Be("111");
        user.Profile.Avatar.Should().Be(uploadedAvatarUrl); // Assert the uploaded URL
        _mockMediator.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Verify upload was called
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserProfileWithEmptyStrings_WhenFieldsAreEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("auth0|123", "old@example.com");
        user.Id = userId; // Set the ID after construction
        user.Profile!.Update(
            "external-id",
            "old@example.com",
            "Old Name",
            "Old",
            "Name",
            "111",
            "old_avatar.png"
        );
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateUserProfileCommand
        {
            Email = "new@example.com",
            FirstName = string.Empty,
            LastName = string.Empty,
            Phone = string.Empty,
            AvatarBase64 = string.Empty // Changed to AvatarBase64
        };
        command.SetId(user.Profile.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Profile.Email.Should().Be(command.Email);
        user.Profile.Name.Should().Be(""); // Both FirstName and LastName are empty
        user.Profile.FirstName.Should().Be(string.Empty);
        user.Profile.LastName.Should().Be(string.Empty);
        user.Profile.Phone.Should().Be(string.Empty);
        user.Profile.Avatar.Should().Be(string.Empty); // Should be empty
        _mockMediator.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Never); // No upload should happen
    }
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAvatarUploadFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("auth0|123", "old@example.com");
        user.Id = userId;
        user.Profile!.Update(
            "external-id",
            "old@example.com",
            "Old Name",
            "Old",
            "Name",
            "111",
            "old_avatar.png"
        );
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync(CancellationToken.None);

        var newAvatarBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";
        var errorMessage = "Upload failed for some reason.";

        var command = new UpdateUserProfileCommand
        {
            Email = "new@example.com",
            AvatarBase64 = newAvatarBase64
        };
        command.SetId(user.Profile.Id);

        _mockMediator.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ImageUploadResponseDto>.Failure(errorMessage, ErrorSources.FileUpload));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.FileUploadFailed);
        result.ErrorSource.Should().Be(ErrorSources.FileUpload);
        _mockMediator.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        user.Profile.Avatar.Should().Be("old_avatar.png"); // Avatar should not change
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenInvalidAvatarBase64Format()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("auth0|123", "old@example.com");
        user.Id = userId;
        user.Profile!.Update(
            "external-id",
            "old@example.com",
            "Old Name",
            "Old",
            "Name",
            "111",
            "old_avatar.png"
        );
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync(CancellationToken.None);

        var invalidBase64 = "this is not a valid base64 string";

        var command = new UpdateUserProfileCommand
        {
            Email = "new@example.com",
            AvatarBase64 = invalidBase64
        };
        command.SetId(user.Profile.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.InvalidBase64);
        result.ErrorSource.Should().Be(ErrorSources.Validation);
        _mockMediator.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Never); // No upload should be attempted
        user.Profile.Avatar.Should().Be("old_avatar.png"); // Avatar should not change
    }
}
