/*
using backend.Domain.Constants;
using backend.Infrastructure.Data;
using backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Models;

namespace backend.Infrastructure.IntegrationTests;

public class ApplicationDbContextInitialiserTests
{
    private readonly Mock<ILogger<ApplicationDbContextInitialiser>> _loggerMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<MongoIdentityRole<ObjectId>>> _roleManagerMock;
    private readonly ApplicationDbContextInitialiser _initialiser;

    public ApplicationDbContextInitialiserTests()
    {
        _loggerMock = new Mock<ILogger<ApplicationDbContextInitialiser>>();
        
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

        // Mock RoleManager
        var roleStoreMock = new Mock<IRoleStore<MongoIdentityRole<ObjectId>>>();
        _roleManagerMock = new Mock<RoleManager<MongoIdentityRole<ObjectId>>>(
            roleStoreMock.Object,
            new List<IRoleValidator<MongoIdentityRole<ObjectId>>>().AsEnumerable(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<ILogger<RoleManager<MongoIdentityRole<ObjectId>>>>().Object
        );

        _initialiser = new ApplicationDbContextInitialiser(
            _loggerMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object
        );
    }

    [Fact]
    public async Task TrySeedAsync_ShouldCreateAdministratorRole_WhenRoleDoesNotExist()
    {
        // Arrange
        _roleManagerMock.Setup(r => r.Roles).Returns(new EnumerableQuery<MongoIdentityRole<ObjectId>>(new List<MongoIdentityRole<ObjectId>>()));
        _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<MongoIdentityRole<ObjectId>>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _initialiser.TrySeedAsync();

        // Assert
        _roleManagerMock.Verify(r => r.CreateAsync(It.Is<MongoIdentityRole<ObjectId>>(role => role.Name == Roles.Administrator)), Times.Once);
    }

    [Fact]
    public async Task TrySeedAsync_ShouldNotCreateAdministratorRole_WhenRoleAlreadyExists()
    {
        // Arrange
        _roleManagerMock.Setup(r => r.Roles).Returns(new EnumerableQuery<MongoIdentityRole<ObjectId>>(new List<MongoIdentityRole<ObjectId>>
        {
            new MongoIdentityRole<ObjectId>(Roles.Administrator)
        }));

        // Act
        await _initialiser.TrySeedAsync();

        // Assert
        _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<MongoIdentityRole<ObjectId>>()), Times.Never);
    }

    [Fact]
    public async Task TrySeedAsync_ShouldCreateAdministratorUser_WhenUserDoesNotExist()
    {
        // Arrange
        _roleManagerMock.Setup(r => r.Roles).Returns(new EnumerableQuery<MongoIdentityRole<ObjectId>>(new List<MongoIdentityRole<ObjectId>>
        {
            new MongoIdentityRole<ObjectId>(Roles.Administrator)
        }));
        _userManagerMock.Setup(u => u.Users).Returns(new EnumerableQuery<ApplicationUser>(new List<ApplicationUser>()));
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<string[]>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _initialiser.TrySeedAsync();

        // Assert
        _userManagerMock.Verify(u => u.CreateAsync(It.Is<ApplicationUser>(user => user.UserName == "administrator@localhost"), "Administrator1!"), Times.Once);
        _userManagerMock.Verify(u => u.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.Is<string[]>(roles => roles.Contains(Roles.Administrator))), Times.Once);
    }

    [Fact]
    public async Task TrySeedAsync_ShouldNotCreateAdministratorUser_WhenUserAlreadyExists()
    {
        // Arrange
        _roleManagerMock.Setup(r => r.Roles).Returns(new EnumerableQuery<MongoIdentityRole<ObjectId>>(new List<MongoIdentityRole<ObjectId>>
        {
            new MongoIdentityRole<ObjectId>(Roles.Administrator)
        }));
        _userManagerMock.Setup(u => u.Users).Returns(new EnumerableQuery<ApplicationUser>(new List<ApplicationUser>
        {
            new ApplicationUser { UserName = "administrator@localhost" }
        }));

        // Act
        await _initialiser.TrySeedAsync();

        // Assert
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(u => u.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<string[]>()), Times.Never);
    }
}
*/
