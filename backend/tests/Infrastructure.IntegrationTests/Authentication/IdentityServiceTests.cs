using backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace backend.Infrastructure.IntegrationTests.Authentication;

public class IdentityServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IUserClaimsPrincipalFactory<ApplicationUser>> _userClaimsPrincipalFactoryMock;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly IdentityService _identityService;

        public IdentityServiceTests()
        {
            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<ApplicationUser>>().Object,
                new List<IUserValidator<ApplicationUser>>().AsEnumerable(),
                new List<IPasswordValidator<ApplicationUser>>().AsEnumerable(),
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object
            );

            // Mock RoleManager (not directly used by IdentityService, but needed for constructor of ApplicationDbContextInitialiserTests)
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object,
                new List<IRoleValidator<IdentityRole>>().AsEnumerable(),
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<IdentityRole>>>().Object
            );

            _userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _authorizationServiceMock = new Mock<IAuthorizationService>();

            _identityService = new IdentityService(
                _userManagerMock.Object,
                _userClaimsPrincipalFactoryMock.Object,
                _authorizationServiceMock.Object
            );
        }

    [Fact]
    public async Task GetUserNameAsync_ShouldReturnUserName_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _identityService.GetUserNameAsync(userId);

        // Assert
        result.Should().Be(user.UserName);
    }

    [Fact]
    public async Task GetUserNameAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _identityService.GetUserNameAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnUserId_WhenCreationSucceeds()
    {
        // Arrange
        var userName = "newuser";
        var password = "Password123!";
        ApplicationUser? capturedUser = null;
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .Callback<ApplicationUser, string>((u, p) => capturedUser = u)
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.FindByNameAsync(userName)).ReturnsAsync(() => capturedUser);

        // Act
        var result = await _identityService.CreateUserAsync(userName, password);

        // Assert
        result.UserId.Should().Be(capturedUser!.Id);
        result.Result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnFailure_WhenCreationFails()
    {
        // Arrange
        var userName = "existinguser";
        var password = "Password123!";
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), password)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));
        _userManagerMock.Setup(u => u.FindByNameAsync(userName)).ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _identityService.CreateUserAsync(userName, password);

        // Assert
        result.Result.IsSuccess.Should().BeFalse();
        result.Result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task IsInRoleAsync_ShouldReturnTrue_WhenUserIsInRole()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleName = "Administrator";
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.IsInRoleAsync(user, roleName)).ReturnsAsync(true);

        // Act
        var result = await _identityService.IsInRoleAsync(userId, roleName);
        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public async Task IsInRoleAsync_ShouldReturnFalse_WhenUserIsNotInRole()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleName = "Administrator";
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.IsInRoleAsync(user, roleName)).ReturnsAsync(false);

        // Act
        var result = await _identityService.IsInRoleAsync(userId, roleName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsInRoleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleName = "Administrator";
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _identityService.IsInRoleAsync(userId, roleName);

        // Assert
        result.Should().BeFalse();
    }



    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFailure_WhenDeletionFails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.DeleteAsync(user)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        // Act
        var result = await _identityService.DeleteUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _identityService.DeleteUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Returns true if user doesn't exist, as the goal (deletion) is achieved.
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldReturnTrue_WhenAuthorizationSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());

        // Act
        var result = await _identityService.AuthorizeAsync(userId, "CanPurge");

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldReturnFalse_WhenAuthorizationFails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        // Act
        var result = await _identityService.AuthorizeAsync(userId, "CanPurge");

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldReturnFalse_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _identityService.AuthorizeAsync(userId, "CanPurge");

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public async Task GetUserNameAsync_ShouldThrowException_WhenFindByIdAsyncThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

        // Act
        Func<Task> act = async () => await _identityService.GetUserNameAsync(userId);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task IsInRoleAsync_ShouldThrowException_WhenFindByIdAsyncThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleName = "Administrator";
        _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

        // Act
        Func<Task> act = async () => await _identityService.IsInRoleAsync(userId, roleName);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldThrowException_WhenCreateClaimsPrincipalFails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _userClaimsPrincipalFactoryMock.Setup(f => f.CreateAsync(user)).ThrowsAsync(new Exception());

        // Act
        Func<Task> act = async () => await _identityService.AuthorizeAsync(userId, "CanPurge");

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task IsInRoleAsync_ShouldThrowException_WhenUserManagerIsInRoleAsyncThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleName = "Administrator";
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.IsInRoleAsync(user, roleName)).ThrowsAsync(new Exception());

        // Act
        Func<Task> act = async () => await _identityService.IsInRoleAsync(userId, roleName);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldThrowException_WhenAuthorizationServiceThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception());

        // Act
        Func<Task> act = async () => await _identityService.AuthorizeAsync(userId, "CanPurge");

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrowException_WhenCreateAsyncThrowsException()
    {
        // Arrange
        var userName = "newuser";
        var password = "Password123!";
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), password)).ThrowsAsync(new Exception());

        // Act
        Func<Task> act = async () => await _identityService.CreateUserAsync(userName, password);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldThrowException_WhenFindByIdAsyncThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

        // Act
        Func<Task> act = async () => await _identityService.DeleteUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldThrowException_WhenDeleteAsyncThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.DeleteAsync(user)).ThrowsAsync(new Exception());

        // Act
        Func<Task> act = async () => await _identityService.DeleteUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}