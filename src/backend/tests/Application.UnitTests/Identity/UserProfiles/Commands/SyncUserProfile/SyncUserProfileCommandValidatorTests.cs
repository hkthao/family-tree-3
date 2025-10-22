using System.Security.Claims;
using backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.SyncUserProfile;

public class SyncUserProfileCommandValidatorTests
{
    private readonly SyncUserProfileCommandValidator _validator;

    public SyncUserProfileCommandValidatorTests()
    {
        _validator = new SyncUserProfileCommandValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenUserPrincipalIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi UserPrincipal là null.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một SyncUserProfileCommand với UserPrincipal là null.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính UserPrincipal với thông báo phù hợp.
        var command = new SyncUserProfileCommand { UserPrincipal = null! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.UserPrincipal)
            .WithErrorMessage("UserPrincipal cannot be null.");
        // 💡 Giải thích: UserPrincipal là trường bắt buộc và không được để null.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenUserPrincipalIsProvided()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi UserPrincipal được cung cấp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một SyncUserProfileCommand với UserPrincipal hợp lệ.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi cho thuộc tính UserPrincipal.
        var command = new SyncUserProfileCommand { UserPrincipal = new ClaimsPrincipal() };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.UserPrincipal);
        // 💡 Giải thích: UserPrincipal hợp lệ không gây ra lỗi.
    }
}
