using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public CreateMemberCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Kiểm tra xem handler có tạo thành công một thành viên mới khi lệnh hợp lệ và người dùng có quyền.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateMember_WhenValidCommandAndUserHasPermission()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF001",
            TotalMembers = 0,
            TotalGenerations = 0
        };

        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        var command = new CreateMemberCommand
        {
            FamilyId = familyId,
            FirstName = "John",
            LastName = "Doe",
            Code = "JD001",
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsRoot = true
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new CreateMemberCommandHandler(handlerContext, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var member = await handlerContext.Members.FindAsync(result.Value);
        member.Should().NotBeNull();
        member!.FamilyId.Should().Be(command.FamilyId);
        member.FirstName.Should().Be(command.FirstName);
        member.LastName.Should().Be(command.LastName);
        member.Code.Should().Be(command.Code);
        member.Gender.Should().Be(command.Gender);
        member.DateOfBirth.Should().Be(command.DateOfBirth);
        member.IsRoot.Should().Be(command.IsRoot);
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về lỗi AccessDenied khi người dùng không có quyền quản lý gia đình.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserLacksPermission()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false); // User lacks permission

        var command = new CreateMemberCommand
        {
            FamilyId = familyId,
            FirstName = "John",
            LastName = "Doe",
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new CreateMemberCommandHandler(handlerContext, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.AccessDenied);
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về lỗi NotFound khi không tìm thấy gia đình.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenFamilyDoesNotExist()
    {
        // Arrange
        var familyId = Guid.NewGuid(); // Family này sẽ không được thêm vào context
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true); // Giả sử người dùng có quyền

        var command = new CreateMemberCommand
        {
            FamilyId = familyId,
            FirstName = "John",
            LastName = "Doe",
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new CreateMemberCommandHandler(handlerContext, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Family with ID {familyId} not found.");
    }

    /// <summary>
    /// Kiểm tra xem handler có đặt thành viên mới làm gốc và bỏ đặt thành viên gốc cũ khi thành viên mới là gốc và thành viên gốc cũ tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldSetNewMemberAsRootAndUnsetRoot_WhenNewMemberIsRootAndOldRootExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF001",
            TotalMembers = 0,
            TotalGenerations = 0
        };

        var oldRootMember = new Member("Old", "Root", "OR001", familyId)
        {
            Id = Guid.NewGuid(),
            IsRoot = true
        };

        _context.Families.Add(existingFamily);
        _context.Members.Add(oldRootMember);
        await _context.SaveChangesAsync();

        var command = new CreateMemberCommand
        {
            FamilyId = familyId,
            FirstName = "New",
            LastName = "Root",
            Code = "NR001",
            Gender = "Female",
            DateOfBirth = new DateTime(1995, 5, 5),
            IsRoot = true
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new CreateMemberCommandHandler(handlerContext, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var newRootMember = await handlerContext.Members.FindAsync(result.Value);
        newRootMember.Should().NotBeNull();
        newRootMember!.IsRoot.Should().BeTrue();

        var updatedOldRootMember = await handlerContext.Members.FindAsync(oldRootMember.Id);
        updatedOldRootMember.Should().NotBeNull();
        updatedOldRootMember!.IsRoot.Should().BeFalse();
    }
}
