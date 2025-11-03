using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.DeleteMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public DeleteMemberCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Kiểm tra xem handler có xóa thành công một thành viên hiện có khi lệnh hợp lệ và người dùng có quyền.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldDeleteMember_WhenValidCommandAndUserHasPermission()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF001",
            TotalMembers = 1,
            TotalGenerations = 1
        };

        var existingMember = new Member("LastName", "FirstName", "MEM001", familyId)
        {
            Id = memberId,
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsRoot = true
        };

        _context.Families.Add(existingFamily);
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        var command = new DeleteMemberCommand(memberId);

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new DeleteMemberCommandHandler(handlerContext, _authorizationServiceMock.Object, _currentUserMock.Object, _mockDateTime.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedMember = await handlerContext.Members.FindAsync(memberId);
        deletedMember.Should().BeNull();
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về lỗi NotFound khi không tìm thấy thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        var memberId = Guid.NewGuid(); // Member này sẽ không tồn tại

        var command = new DeleteMemberCommand(memberId);

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new DeleteMemberCommandHandler(handlerContext, _authorizationServiceMock.Object, _currentUserMock.Object, _mockDateTime.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {memberId} not found.");
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về lỗi AccessDenied khi người dùng không có quyền quản lý gia đình của thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserLacksPermission()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false); // User lacks permission

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF001",
            TotalMembers = 1,
            TotalGenerations = 1
        };

        var existingMember = new Member("LastName", "FirstName", "MEM001", familyId)
        {
            Id = memberId,
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsRoot = true
        };

        _context.Families.Add(existingFamily);
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        var command = new DeleteMemberCommand(memberId);

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new DeleteMemberCommandHandler(handlerContext, _authorizationServiceMock.Object, _currentUserMock.Object, _mockDateTime.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.AccessDenied);
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về lỗi NotFound khi không tìm thấy gia đình của thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenFamilyDoesNotExistForMember()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true); // User has permission

        // Only add member, not family
        var existingMember = new Member("LastName", "FirstName", "MEM001", familyId)
        {
            Id = memberId,
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsRoot = true
        };

        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        var command = new DeleteMemberCommand(memberId);

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new DeleteMemberCommandHandler(handlerContext, _authorizationServiceMock.Object, _currentUserMock.Object, _mockDateTime.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Family with ID {familyId} not found.");
    }
}
