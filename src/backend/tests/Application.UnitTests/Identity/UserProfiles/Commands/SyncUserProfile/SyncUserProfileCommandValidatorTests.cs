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

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi UserPrincipal của SyncUserProfileCommand là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một SyncUserProfileCommand với UserPrincipal được đặt thành null.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính UserPrincipal với thông báo lỗi cụ thể "UserPrincipal cannot be null.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: UserPrincipal là một trường bắt buộc và không được phép có giá trị null để đảm bảo thông tin người dùng hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenUserPrincipalIsNull()
    {
        var command = new SyncUserProfileCommand { UserPrincipal = null! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.UserPrincipal)
            .WithErrorMessage("UserPrincipal cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator không báo lỗi khi UserPrincipal của SyncUserProfileCommand được cung cấp hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một SyncUserProfileCommand với UserPrincipal được đặt thành một ClaimsPrincipal hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng không có lỗi xác thực nào cho thuộc tính UserPrincipal.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một UserPrincipal hợp lệ nên vượt qua quá trình xác thực mà không có bất kỳ lỗi nào.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenUserPrincipalIsProvided()
    {
        var command = new SyncUserProfileCommand { UserPrincipal = new ClaimsPrincipal() };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.UserPrincipal);
    }
}
