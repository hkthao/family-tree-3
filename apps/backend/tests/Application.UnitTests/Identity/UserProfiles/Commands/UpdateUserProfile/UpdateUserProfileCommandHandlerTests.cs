using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

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

        var command = new UpdateUserProfileCommand
        {
            Email = "new@example.com",
            FirstName = "New",
            LastName = "User",
            Phone = "222"
        };
        command.SetId(user.Profile.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Profile.Email.Should().Be(command.Email);
        user.Profile.Name.Should().Be("New User");
        user.Profile.FirstName.Should().Be(command.FirstName);
        user.Profile.LastName.Should().Be(command.LastName);
        user.Profile.Phone.Should().Be(command.Phone);
        user.Profile.Avatar.Should().Be("old_avatar.png"); // Should retain old avatar as no new avatar is provided
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
            Email = "new@example.com",
            FirstName = null, // Should retain old value
            LastName = "Updated",
            Phone = null // Should retain old value
        };

        command.SetId(user.Profile.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Profile.Email.Should().Be(command.Email);
        user.Profile.Name.Should().Be("Old Updated"); // FirstName is null, so it uses old FirstName + new LastName
        user.Profile.FirstName.Should().Be("Old");
        user.Profile.LastName.Should().Be(command.LastName);
        user.Profile.Phone.Should().Be("111");
        user.Profile.Avatar.Should().Be("old_avatar.png"); // Should retain old avatar as no new avatar is provided
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
            Phone = string.Empty
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
        user.Profile.Avatar.Should().Be("old_avatar.png"); // Should retain old avatar as no new avatar is provided
    }


}
