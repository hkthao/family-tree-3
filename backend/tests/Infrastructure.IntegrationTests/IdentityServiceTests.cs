using backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace backend.Infrastructure.IntegrationTests;

public class IdentityServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<MongoIdentityRole<ObjectId>>> _roleManagerMock;
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
            var roleStoreMock = new Mock<IRoleStore<MongoIdentityRole<ObjectId>>>();
            _roleManagerMock = new Mock<RoleManager<MongoIdentityRole<ObjectId>>>(
                roleStoreMock.Object,
                new List<IRoleValidator<MongoIdentityRole<ObjectId>>>().AsEnumerable(),
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<MongoIdentityRole<ObjectId>>>>().Object
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
        var userId = "testUserId";
        var user = new ApplicationUser { Id = ObjectId.Parse(userId), UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(ObjectId.Parse(userId).ToString())).ReturnsAsync(user);

        // Act
        var result = await _identityService.GetUserNameAsync(userId);

        // Assert
        result.Should().Be(user.UserName);
    }

    [Fact]
    public async Task GetUserNameAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonExistentUserId";
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
        var user = new ApplicationUser { UserName = userName };
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), password)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.FindByNameAsync(userName)).ReturnsAsync(user);

        // Act
        var result = await _identityService.CreateUserAsync(userName, password);

        // Assert
        result.UserId.Should().Be(user.Id);
        result.Result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnFailure_WhenCreationFails()
    {
        // Arrange
        var userName = "existinguser";
        var password = "Password123!";
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), password)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        // Act
        var result = await _identityService.CreateUserAsync(userName, password);

        // Assert
        result.UserId.Should().Be(ObjectId.Empty);
        result.Result.Succeeded.Should().BeFalse();
        result.Result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task IsInRoleAsync_ShouldReturnTrue_WhenUserIsInRole()
    {
        // Arrange
        var userId = "testUserId";
        var roleName = "Administrator";
        var user = new ApplicationUser { Id = ObjectId.Parse(userId), UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        var result = await _identityService.IsInRoleAsync(ObjectId.Parse(userId), roleName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsInRoleAsync_ShouldReturnFalse_WhenUserIsNotInRole()
    {
        // Arrange
        var userId = "testUserId";
        var roleName = "Administrator";
        var user = new ApplicationUser { Id = ObjectId.Parse(userId), UserName = "testuser" };
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
        var userId = "nonExistentUserId";
        var roleName = "Administrator";
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _identityService.IsInRoleAsync(userId, roleName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnSuccess_WhenDeletionSucceeds()
    {
        // Arrange
        var userId = "testUserId";
        var user = new ApplicationUser { Id = ObjectId.Parse(userId), UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _identityService.DeleteUserAsync(ObjectId.Parse(userId));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFailure_WhenDeletionFails()
    {
        // Arrange
        var userId = "testUserId";
        var user = new ApplicationUser { Id = ObjectId.Parse(userId), UserName = "testuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.DeleteAsync(user)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        // Act
        var result = await _identityService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonExistentUserId";
        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _identityService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeTrue(); // Returns true if user doesn't exist, as the goal (deletion) is achieved.
    }
}
