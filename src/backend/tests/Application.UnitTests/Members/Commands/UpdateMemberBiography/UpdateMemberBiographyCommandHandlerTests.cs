using backend.Application.Common.Constants;
using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.UpdateMemberBiography;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMemberBiography;

/// <summary>
/// Bộ test cho UpdateMemberBiographyCommandHandler.
/// </summary>
public class UpdateMemberBiographyCommandHandlerTests : TestBase
{
    private readonly UpdateMemberBiographyCommandHandler _handler;

    public UpdateMemberBiographyCommandHandlerTests()
    {
        _handler = new UpdateMemberBiographyCommandHandler(
            _context,
            _mockAuthorizationService.Object
        );
    }


    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy thành viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Mock _mockUser.Id trả về một ID hợp lệ. Đảm bảo không có thành viên nào trong DB.
    ///    - Act: Gọi phương thức Handle với một UpdateMemberBiographyCommand bất kỳ.
    ///    - Assert: Kiểm tra kết quả trả về là thất bại, với thông báo lỗi là ErrorMessages.NotFound
    ///              và ErrorSource là ErrorSources.NotFound.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải kiểm tra sự tồn tại của thành viên trước khi cập nhật.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        // No member added to _context.Members, so FindAsync will return null

        var command = _fixture.Create<UpdateMemberBiographyCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Member with ID {command.MemberId}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng không có quyền.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Mock _mockUser.Id trả về một ID hợp lệ. Thêm một thành viên vào DB.
    ///               Mock _mockAuthorizationService.CanAccessFamily trả về false và _mockAuthorizationService.IsAdmin trả về false.
    ///    - Act: Gọi phương thức Handle với một UpdateMemberBiographyCommand.
    ///    - Assert: Kiểm tra kết quả trả về là thất bại, với thông báo lỗi là ErrorMessages.AccessDenied
    ///              và ErrorSource là ErrorSources.Forbidden.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải kiểm tra quyền truy cập của người dùng trước khi cập nhật thông tin thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAuthorizationFails()
    {
        // Arrange
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family Name", Code = "TF001" }; // Manually create Family
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var memberId = Guid.NewGuid();
        var member = new Member { Id = memberId, FamilyId = familyId, FirstName = "John", LastName = "Doe", Biography = "Some bio", Code = "M001" }; // Manually create Member
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(a => a.CanAccessFamily(It.IsAny<Guid>())).Returns(false);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false); // Ensure not admin

        var command = new UpdateMemberBiographyCommand { MemberId = member.Id, BiographyContent = _fixture.Create<string>() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh handler cập nhật tiểu sử thành viên thành công khi người dùng được ủy quyền.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập dữ liệu thủ công cho Family, Member, UserProfile, FamilyUser.
    ///               Mock _mockUser.Id và _mockAuthorizationService.CanAccessFamily trả về true.
    ///    - Act: Gọi phương thức Handle với một UpdateMemberBiographyCommand.
    ///    - Assert: Kiểm tra kết quả trả về là thành công. Xác minh tiểu sử của thành viên được cập nhật.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải cập nhật thành công các thuộc tính của hồ sơ người dùng và lưu các thay đổi vào cơ sở dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateMemberBiographySuccessfully_WhenAuthorized()
    {
        // Manual Data Setup
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userProfileId = Guid.NewGuid();
        var newBiographyContent = "This is a new biography content for the member.";

        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF001" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var member = new Member { Id = memberId, FamilyId = familyId, FirstName = "John", LastName = "Doe", Biography = "Old biography", Code = "M001" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var userProfile = new UserProfile { Id = userProfileId, ExternalId = "external-user-id", Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        var familyUser = new FamilyUser { FamilyId = familyId, UserProfileId = userProfileId, Role = FamilyRole.Manager };
        _context.FamilyUsers.Add(familyUser);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(userProfileId);
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false); // Not an admin

        var command = new UpdateMemberBiographyCommand
        {
            MemberId = memberId,
            BiographyContent = newBiographyContent
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var updatedMember = await _context.Members.FindAsync(memberId);
        updatedMember.Should().NotBeNull();
        updatedMember!.Biography.Should().Be(newBiographyContent);
    }
}
