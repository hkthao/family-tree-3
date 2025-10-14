using backend.Application.UnitTests.Common;
using Xunit;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Application.Families.Commands.CreateFamily;
using backend.Domain.Entities;
using backend.Application.Common.Models;
using backend.Application.UserProfiles.Specifications;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandlerTests : TestBase
{
    private readonly CreateFamilyCommandHandler _handler;

    public CreateFamilyCommandHandlerTests()
    {
        _handler = new CreateFamilyCommandHandler(
            _context,
            _mockUser.Object,
            _mockMediator.Object,
            _mockFamilyTreeService.Object);
    }

    /// <summary>
    /// Kiểm tra xem một gia đình có được tạo thành công khi tất cả các điều kiện hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh CreateFamilyCommand hợp lệ được cung cấp
    /// và người dùng được xác thực với UserProfile tồn tại, một gia đình mới sẽ được tạo,
    /// người dùng được gán vai trò quản lý, và các hoạt động liên quan được ghi lại.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Create_Family_Successfully()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Xóa tất cả các gia đình, thành viên và người dùng hiện có để đảm bảo môi trường test sạch.
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        _mockUser.Setup(x => x.Id).Returns(userId);

        var userProfile = new UserProfile { Id = userProfileId, ExternalId = userId, Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Mock UserProfileByExternalIdSpecification để trả về userProfile đã tạo.
        // Note: Việc mock Specification trực tiếp trong unit test có thể phức tạp.
        // Thay vào đó, chúng ta sẽ đảm bảo userProfile tồn tại trong context.

        var command = new CreateFamilyCommand
        {
            Name = "Gia đình Mới",
            Description = "Mô tả gia đình mới",
            Address = "Địa chỉ mới",
            AvatarUrl = "http://example.com/avatar.jpg",
            Visibility = "Public"
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo lệnh thực thi thành công.
        result.IsSuccess.Should().BeTrue();
        var familyId = result.Value;

        // Đảm bảo gia đình mới được tạo và lưu vào cơ sở dữ liệu.
        var createdFamily = await _context.Families.FindAsync(familyId);
        createdFamily.Should().NotBeNull();
        createdFamily!.Name.Should().Be("Gia đình Mới");

        // Đảm bảo FamilyUser được tạo và gán vai trò Manager.
        var familyUser = await _context.FamilyUsers.FirstOrDefaultAsync(fu => fu.FamilyId == familyId && fu.UserProfileId == userProfileId);
        familyUser.Should().NotBeNull();
        familyUser!.Role.Should().Be(FamilyRole.Manager);

        // Đảm bảo RecordActivityCommand và UpdateFamilyStats được gọi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(familyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng hiện tại không được xác thực.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng nếu _user.Id là null hoặc rỗng, handler sẽ trả về kết quả thất bại
    /// với thông báo lỗi xác thực.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthenticated()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Xóa tất cả các gia đình, thành viên và người dùng hiện có để đảm bảo môi trường test sạch.
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Giả lập _user.Id là null hoặc rỗng.
        _mockUser.Setup(x => x.Id).Returns((string)null!);

        var command = new CreateFamilyCommand
        {
            Name = "Gia đình",
            Description = "Mô tả",
            Visibility = "Private"
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo lệnh thực thi thất bại và thông báo lỗi chính xác.
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Current user ID not found.");
        // Đảm bảo RecordActivityCommand và UpdateFamilyStats không được gọi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi UserProfile của người dùng không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng nếu UserProfile không được tìm thấy cho người dùng hiện tại,
    /// handler sẽ trả về kết quả thất bại với thông báo lỗi "không tìm thấy".
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserProfileNotFound()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Xóa tất cả các gia đình, thành viên và người dùng hiện có để đảm bảo môi trường test sạch.
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        var userId = Guid.NewGuid().ToString();
        _mockUser.Setup(x => x.Id).Returns(userId);

        // Không thêm UserProfile vào context, giả lập không tìm thấy.

        var command = new CreateFamilyCommand
        {
            Name = "Gia đình",
            Description = "Mô tả",
            Visibility = "Private"
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo lệnh thực thi thất bại và thông báo lỗi chính xác.
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        // Đảm bảo RecordActivityCommand và UpdateFamilyStats không được gọi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}