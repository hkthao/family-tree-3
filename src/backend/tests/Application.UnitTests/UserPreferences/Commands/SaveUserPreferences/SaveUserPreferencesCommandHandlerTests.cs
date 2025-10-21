
using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UnitTests.Common;
using backend.Application.UserPreferences.Commands.SaveUserPreferences;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.UserPreferences.Commands.SaveUserPreferences;

public class SaveUserPreferencesCommandHandlerTests : TestBase
{
    /// <summary>
    /// Mục tiêu: Kiểm tra rằng handler có thể cập nhật các thiết lập người dùng (UserPreferences) thành công
    /// khi người dùng đã có thiết lập từ trước.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateExistingPreferences_WhenRequestIsValid()
    {
        // ARRANGE
        // 1. Tạo một UserProfile và một UserPreference đã tồn tại trong cơ sở dữ liệu.
        //    - Tạo đối tượng UserProfile thủ công.
        //    - Lưu vào DbContext để EF Core gán một Id.
        var userProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            ExternalId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User"
        };
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        // 2. Thiết lập Mock cho IUser service để trả về ExternalId của người dùng vừa tạo.
        //    - Điều này giả lập rằng người dùng đang đăng nhập chính là người dùng này.
        _mockUser.Setup(u => u.Id).Returns(userProfile.ExternalId);

        // 3. Tạo một UserPreference đã tồn tại, liên kết với UserProfile ở trên.
        var existingPreference = new UserPreference
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfile.Id,
            Language = Language.English, // Giá trị ban đầu
            Theme = Theme.Light // Giá trị ban đầu
        };
        _context.UserPreferences.Add(existingPreference);
        await _context.SaveChangesAsync();
        // 4. Tạo command với dữ liệu mới để cập nhật.
        var command = new SaveUserPreferencesCommand
        {
            Language = Language.Vietnamese, // Giá trị mới
            Theme = Theme.Dark // Giá trị mới
        };

        // 5. Khởi tạo handler với các dependency cần thiết.
        var handler = new SaveUserPreferencesCommandHandler(_context, _mockUser.Object, _mapper);

        // ACT
        // Thực thi handler để cập nhật dữ liệu.
        var result = await handler.Handle(command, CancellationToken.None);

        // ASSERT
        // 1. Kiểm tra kết quả trả về phải là thành công.
        result.IsSuccess.Should().BeTrue();

        // 2. Tìm lại UserPreference trong DB để xác nhận thay đổi bằng một context mới.
        var updatedPreference = await _context.UserPreferences.FirstOrDefaultAsync(e => e.Id == existingPreference.Id);

        // 3. Các thuộc tính phải được cập nhật chính xác theo giá trị mới từ command.
        //    - Giải thích: Điều này khẳng định logic cập nhật của handler hoạt động đúng
        //      khi tìm thấy một preference đã tồn tại.
        updatedPreference.Should().NotBeNull();
        updatedPreference!.Language.Should().Be(command.Language);
        updatedPreference!.Theme.Should().Be(command.Theme);
    }

    /// <summary>
    /// Mục tiêu: Kiểm tra rằng handler sẽ tạo mới UserPreference nếu chưa tồn tại
    /// cho người dùng hiện tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateNewPreferences_WhenNoneExist()
    {
        // ARRANGE
        // 1. Tạo UserProfile nhưng KHÔNG tạo UserPreference tương ứng.
        var userProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            ExternalId = Guid.NewGuid().ToString(),
            Email = "newuser@example.com",
            Name = "New User"
        };
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        // 2. Mock IUser service để giả lập người dùng này đang đăng nhập.
        _mockUser.Setup(u => u.Id).Returns(userProfile.ExternalId);

        // 3. Tạo command với các giá trị cần lưu.
        var command = new SaveUserPreferencesCommand
        {
            Language = Language.Vietnamese,
            Theme = Theme.Dark
        };

        // 4. Khởi tạo handler.
        var handler = new SaveUserPreferencesCommandHandler(_context, _mockUser.Object, _mapper);

        // ACT
        // Thực thi handler.
        var result = await handler.Handle(command, CancellationToken.None);

        // ASSERT
        // 1. Kết quả phải thành công.
        result.IsSuccess.Should().BeTrue();

        // 2. Phải có một UserPreference mới được tạo trong DB bằng một context mới.
        using var queryContext = CreateNewContext();
        var newPreference = await queryContext.UserPreferences.FirstOrDefaultAsync(p => p.UserProfile.Id == userProfile.Id);

        // 3. Dữ liệu của preference mới phải khớp với command.
        //    - Giải thích: Điều này xác nhận logic của handler có thể xử lý trường hợp
        //      tạo mới preference khi người dùng lần đầu tiên lưu thiết lập.
        newPreference.Should().NotBeNull();
        newPreference!.Language.Should().Be(command.Language);
        newPreference!.Theme.Should().Be(command.Theme);
    }

    /// <summary>
    /// Mục tiêu: Kiểm tra rằng handler trả về lỗi khi không thể xác thực người dùng (user ID is null).
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAuthenticated()
    {
        // ARRANGE
        // 1. Mock IUser service để trả về null, giả lập người dùng chưa đăng nhập.
        _mockUser.Setup(u => u.Id).Returns((string)null!);

        // 2. Tạo command.
        var command = new SaveUserPreferencesCommand();

        // 3. Khởi tạo handler.
        var handler = new SaveUserPreferencesCommandHandler(_context, _mockUser.Object, _mapper);

        // ACT
        // Thực thi handler.
        var result = await handler.Handle(command, CancellationToken.None);

        // ASSERT
        // 1. Kết quả phải là thất bại.
        result.IsSuccess.Should().BeFalse();

        // 2. Phải có thông điệp lỗi rõ ràng về việc xác thực.
        //    - Giải thích: Đây là một bước kiểm tra bảo mật quan trọng, đảm bảo rằng
        //      không có hành động nào được thực hiện nếu không xác định được danh tính người dùng.
        result.Error.Should().Contain("User is not authenticated.");
    }
}
