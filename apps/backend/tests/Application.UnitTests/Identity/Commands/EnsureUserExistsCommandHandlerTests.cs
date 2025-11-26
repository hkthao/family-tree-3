using backend.Application.Identity.Commands.EnsureUserExists;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Identity.Commands;

public class EnsureUserExistsCommandHandlerTests : TestBase
{
    private readonly Mock<ILogger<EnsureUserExistsCommandHandler>> _loggerMock;
    private readonly EnsureUserExistsCommandHandler _handler;

    public EnsureUserExistsCommandHandlerTests()
    {
        _loggerMock = new Mock<ILogger<EnsureUserExistsCommandHandler>>();
        _handler = new EnsureUserExistsCommandHandler(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingUser_WhenUserExists()
    {
        // Arrange
        var authProviderId = "auth0|existinguser";
        var existingUser = new User(authProviderId, "existing@test.com", "Existing User", "Existing", "User", "123", "avatar.png");
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var command = new EnsureUserExistsCommand { ExternalId = authProviderId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.UserId.Should().Be(existingUser.Id);
        result.ProfileId.Should().Be(existingUser.Profile!.Id);
        var userCount = await _context.Users.CountAsync();
        userCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCreateNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var authProviderId = "auth0|newuser";
        var command = new EnsureUserExistsCommand
        {
            ExternalId = authProviderId,
            Email = "new@test.com",
            Name = "New User",
            FirstName = "New",
            LastName = "User",
            Phone = "456",
            Avatar = "new_avatar.png"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.UserId.Should().NotBeEmpty();
        var newUser = await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == result.UserId);
        newUser.Should().NotBeNull();
        newUser!.AuthProviderId.Should().Be(authProviderId);
        newUser.Email.Should().Be(command.Email);
        newUser.Profile.Should().NotBeNull();
        newUser.Profile!.Name.Should().Be(command.Name);
        newUser.Profile.FirstName.Should().Be(command.FirstName);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenExternalIdIsNull()
    {
        // Arrange
        var command = new EnsureUserExistsCommand { ExternalId = null };

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
