using backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandValidatorTests
{
    private readonly UpdateUserProfileCommandValidator _validator;

    public UpdateUserProfileCommandValidatorTests()
    {
        _validator = new UpdateUserProfileCommandValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi thuộc tính Id của UpdateUserProfileCommand là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Id được đặt thành null.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Id với thông báo lỗi cụ thể "Id cannot be null.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Id là một trường bắt buộc và không được phép có giá trị null để xác định hồ sơ người dùng cần cập nhật.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenIdIsNull()
    {
        var command = new UpdateUserProfileCommand { Id = null! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Id)
            .WithErrorMessage("Id cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi thuộc tính Id của UpdateUserProfileCommand là một chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Id được đặt thành string.Empty.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Id với thông báo lỗi cụ thể "Id cannot be empty.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Id là một trường bắt buộc và không được phép có giá trị rỗng để xác định hồ sơ người dùng cần cập nhật.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenIdIsEmpty()
    {
        var command = new UpdateUserProfileCommand { Id = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Id)
            .WithErrorMessage("Id cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi thuộc tính Name của UpdateUserProfileCommand là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Name được đặt thành null.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Name với thông báo lỗi cụ thể "Name cannot be null.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name là một trường bắt buộc và không được phép có giá trị null để đảm bảo hồ sơ người dùng có tên hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenNameIsNull()
    {
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = null! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Name cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi thuộc tính Name của UpdateUserProfileCommand là một chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Name được đặt thành string.Empty.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Name với thông báo lỗi cụ thể "Name cannot be empty.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name là một trường bắt buộc và không được phép có giá trị rỗng để đảm bảo hồ sơ người dùng có tên hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Name cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi thuộc tính Name của UpdateUserProfileCommand vượt quá độ dài tối đa cho phép (256 ký tự).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Name là một chuỗi dài hơn 256 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Name với thông báo lỗi cụ thể "Name must not exceed 256 characters.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name có giới hạn độ dài để đảm bảo tính nhất quán và hiệu quả lưu trữ dữ liệu.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenNameExceedsMaxLength()
    {
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = new string('a', 257) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Name must not exceed 256 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi thuộc tính Email của UpdateUserProfileCommand là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Email được đặt thành null.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Email với thông báo lỗi cụ thể "Email cannot be null.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Email là một trường bắt buộc và không được phép có giá trị null để đảm bảo hồ sơ người dùng có địa chỉ email hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenEmailIsNull()
    {
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = null! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi thuộc tính Email của UpdateUserProfileCommand là một chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Email được đặt thành string.Empty.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Email với thông báo lỗi cụ thể "Email cannot be empty.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Email là một trường bắt buộc và không được phép có giá trị rỗng để đảm bảo hồ sơ người dùng có địa chỉ email hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenEmailIsEmpty()
    {
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email cannot be empty.");
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi thuộc tính Email của UpdateUserProfileCommand không phải là một địa chỉ email hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Email được đặt thành một chuỗi không phải là định dạng email hợp lệ (ví dụ: "invalid-email").
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Email với thông báo lỗi cụ thể "Email must be a valid email address.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Email phải tuân thủ định dạng email chuẩn để đảm bảo tính hợp lệ và khả năng gửi thông báo.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenEmailIsInvalid()
    {
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = "invalid-email" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email must be a valid email address.");
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi thuộc tính Email của UpdateUserProfileCommand vượt quá độ dài tối đa cho phép (256 ký tự).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Email là một chuỗi dài hơn 256 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Email với thông báo lỗi cụ thể "Email must not exceed 256 characters.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Email có giới hạn độ dài để đảm bảo tính nhất quán và hiệu quả lưu trữ dữ liệu.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenEmailExceedsMaxLength()
    {
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = new string('a', 250) + "@example.com" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email must not exceed 256 characters.");
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi thuộc tính Avatar của UpdateUserProfileCommand vượt quá độ dài tối đa cho phép (2048 ký tự).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Avatar là một chuỗi URL dài hơn 2048 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Avatar với thông báo lỗi cụ thể "Avatar URL must not exceed 2048 characters.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Avatar URL có giới hạn độ dài để đảm bảo tính nhất quán và hiệu quả lưu trữ dữ liệu.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenAvatarExceedsMaxLength()
    {
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = "test@example.com", Avatar = "http://example.com/" + new string('a', 2030) + ".jpg" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Avatar)
            .WithErrorMessage("Avatar URL must not exceed 2048 characters.");
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi thuộc tính Avatar của UpdateUserProfileCommand không phải là một URL hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với Avatar được đặt thành một chuỗi không phải là URL hợp lệ (ví dụ: "invalid-url").
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Avatar với thông báo lỗi cụ thể "Avatar URL must be a valid URL.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Avatar URL phải tuân thủ định dạng URL chuẩn để đảm bảo tính hợp lệ và khả năng truy cập.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenAvatarIsInvalidUrl()
    {
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = "test@example.com", Avatar = "invalid-url" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Avatar)
            .WithErrorMessage("Avatar URL must be a valid URL.");
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator không báo lỗi khi tất cả các thuộc tính của UpdateUserProfileCommand đều hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateUserProfileCommand với tất cả các thuộc tính (Id, Name, Email, Avatar) được đặt thành các giá trị hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng không có bất kỳ lỗi xác thực nào được báo cáo.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một command hợp lệ với tất cả các trường tuân thủ quy tắc xác thực nên được chấp nhận mà không có lỗi.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenValidCommand()
    {
        var command = new UpdateUserProfileCommand
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Valid Name",
            Email = "valid@example.com",
            Avatar = "http://valid.com/avatar.jpg"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
