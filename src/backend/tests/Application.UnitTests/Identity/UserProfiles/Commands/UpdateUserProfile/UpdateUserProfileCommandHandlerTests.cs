using backend.Application.Common.Constants;
using backend.Application.Identity.Commands.UpdateUserProfile;
using backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandlerTests : TestBase
{
    private readonly UpdateUserProfileCommandHandler _handler;

    public UpdateUserProfileCommandHandlerTests()
    {
        _handler = new UpdateUserProfileCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenInvalidUserIdFormat()
    {
        // Arrange
        var command = new UpdateUserProfileCommand { Id = "invalid-guid" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.InvalidUserIdFormat);
        result.ErrorSource.Should().Be(ErrorSources.Validation);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand { Id = userId.ToString() };

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
            Id = userId.ToString(),
            Email = "new@example.com",
            FirstName = "New",
            LastName = "User",
            Phone = "222",
            Avatar = "new_avatar.png"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Profile.Email.Should().Be(command.Email);
        user.Profile.Name.Should().Be("New User");
        user.Profile.FirstName.Should().Be(command.FirstName);
        user.Profile.LastName.Should().Be(command.LastName);
        user.Profile.Phone.Should().Be(command.Phone);
        user.Profile.Avatar.Should().Be(command.Avatar);
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

        var command = new UpdateUserProfileCommand
        {
            Id = userId.ToString(),
            Email = "new@example.com",
            FirstName = null, // Should retain old value
            LastName = "Updated",
            Phone = null, // Should retain old value
            Avatar = "new_avatar.png"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Profile.Email.Should().Be(command.Email);
        user.Profile.Name.Should().Be("Old Updated"); // FirstName is null, so it uses old FirstName + new LastName
        user.Profile.FirstName.Should().Be("Old");
        user.Profile.LastName.Should().Be(command.LastName);
        user.Profile.Phone.Should().Be("111");
        user.Profile.Avatar.Should().Be(command.Avatar);
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
            Id = userId.ToString(),
            Email = "new@example.com",
            FirstName = string.Empty,
            LastName = string.Empty,
            Phone = string.Empty,
            Avatar = string.Empty
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Profile.Email.Should().Be(command.Email);
        user.Profile.Name.Should().Be(""); // Both FirstName and LastName are empty
        user.Profile.FirstName.Should().Be(string.Empty);
        user.Profile.LastName.Should().Be(string.Empty);
        user.Profile.Phone.Should().Be(string.Empty);
        user.Profile.Avatar.Should().Be(string.Empty);
    }
}