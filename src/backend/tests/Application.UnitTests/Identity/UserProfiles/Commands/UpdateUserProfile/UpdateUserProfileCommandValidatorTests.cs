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

    [Fact]
    public void ShouldHaveErrorWhenIdIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Id là null.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Id là null.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Id với thông báo phù hợp.
        var command = new UpdateUserProfileCommand { Id = null! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Id)
            .WithErrorMessage("Id cannot be null.");
        // 💡 Giải thích: Id là trường bắt buộc và không được để null.
    }

    [Fact]
    public void ShouldHaveErrorWhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Id là chuỗi rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Id là chuỗi rỗng.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Id với thông báo phù hợp.
        var command = new UpdateUserProfileCommand { Id = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Id)
            .WithErrorMessage("Id cannot be empty.");
        // 💡 Giải thích: Id là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldHaveErrorWhenNameIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Name là null.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Name là null.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Name với thông báo phù hợp.
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = null! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Name cannot be null.");
        // 💡 Giải thích: Name là trường bắt buộc và không được để null.
    }

    [Fact]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Name là chuỗi rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Name là chuỗi rỗng.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Name với thông báo phù hợp.
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Name cannot be empty.");
        // 💡 Giải thích: Name là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldHaveErrorWhenNameExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Name vượt quá độ dài tối đa.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Name dài hơn 256 ký tự.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Name với thông báo phù hợp.
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = new string('a', 257) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Name must not exceed 256 characters.");
        // 💡 Giải thích: Name không được vượt quá 256 ký tự.
    }

    [Fact]
    public void ShouldHaveErrorWhenEmailIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Email là null.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Email là null.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Email với thông báo phù hợp.
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = null! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email cannot be null.");
        // 💡 Giải thích: Email là trường bắt buộc và không được để null.
    }

    [Fact]
    public void ShouldHaveErrorWhenEmailIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Email là chuỗi rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Email là chuỗi rỗng.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Email với thông báo phù hợp.
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email cannot be empty.");
        // 💡 Giải thích: Email là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldHaveErrorWhenEmailIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Email không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Email không hợp lệ.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Email với thông báo phù hợp.
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = "invalid-email" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email must be a valid email address.");
        // 💡 Giải thích: Email phải có định dạng hợp lệ.
    }

    [Fact]
    public void ShouldHaveErrorWhenEmailExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Email vượt quá độ dài tối đa.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Email dài hơn 256 ký tự.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Email với thông báo phù hợp.
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = new string('a', 250) + "@example.com" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email must not exceed 256 characters.");
        // 💡 Giải thích: Email không được vượt quá 256 ký tự.
    }

    [Fact]
    public void ShouldHaveErrorWhenAvatarExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Avatar vượt quá độ dài tối đa.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Avatar URL dài hơn 2048 ký tự.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Avatar với thông báo phù hợp.
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = "test@example.com", Avatar = "http://example.com/" + new string('a', 2030) + ".jpg" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Avatar)
            .WithErrorMessage("Avatar URL must not exceed 2048 characters.");
        // 💡 Giải thích: Avatar URL không được vượt quá 2048 ký tự.
    }

    [Fact]
    public void ShouldHaveErrorWhenAvatarIsInvalidUrl()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Avatar không phải là URL hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với Avatar không hợp lệ.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Avatar với thông báo phù hợp.
        var command = new UpdateUserProfileCommand { Id = Guid.NewGuid().ToString(), Name = "Test Name", Email = "test@example.com", Avatar = "invalid-url" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Avatar)
            .WithErrorMessage("Avatar URL must be a valid URL.");
        // 💡 Giải thích: Avatar URL phải là một URL hợp lệ.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenValidCommand()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi tất cả các trường đều hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateUserProfileCommand với tất cả các trường hợp lệ.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi nào được báo cáo.
        var command = new UpdateUserProfileCommand
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Valid Name",
            Email = "valid@example.com",
            Avatar = "http://valid.com/avatar.jpg"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Một lệnh hợp lệ không nên gây ra bất kỳ lỗi xác thực nào.
    }
}
