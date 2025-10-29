using backend.Application.Families.Commands.UpdateFamily;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

/// <summary>
/// Bộ test cho UpdateFamilyCommandValidator.
/// </summary>
public class UpdateFamilyCommandValidatorTests
{
    private readonly UpdateFamilyCommandValidator _validator;

    public UpdateFamilyCommandValidatorTests()
    {
        _validator = new UpdateFamilyCommandValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Id là Guid rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Id là Guid.Empty.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Id với thông báo lỗi "Id cannot be empty.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Id là trường bắt buộc và không được để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.Empty };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Id hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Id hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Id.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Id hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenIdIsValid()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Name", Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Name là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Name là null.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Name với thông báo lỗi "Name cannot be null.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name là trường bắt buộc và không được để null.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenNameIsNull()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = null!, Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Name là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Name là chuỗi rỗng.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Name với thông báo lỗi "Name cannot be empty.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name là trường bắt buộc và không được để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = string.Empty, Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Name vượt quá 200 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Name dài hơn 200 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Name với thông báo lỗi "Name must not exceed 200 characters.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name có giới hạn độ dài tối đa là 200 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = new string('a', 201), Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 200 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Name hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Name hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Name.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenNameIsValid()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Family Name", Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Description vượt quá 1000 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Description dài hơn 1000 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Description với thông báo lỗi "Description must not exceed 1000 characters.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Description có giới hạn độ dài tối đa là 1000 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Description = new string('a', 1001), Name = "Valid Name", Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description must not exceed 1000 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Description hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Description hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Description.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Description hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenDescriptionIsValid()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Description = "Valid description", Name = "Valid Name", Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Address vượt quá 500 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Address dài hơn 500 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Address với thông báo lỗi "Address must not exceed 500 characters.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Address có giới hạn độ dài tối đa là 500 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenAddressExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Address = new string('a', 501), Name = "Valid Name", Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address)
              .WithErrorMessage("Address must not exceed 500 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Address hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Address hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Address.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Address hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenAddressIsValid()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Address = "Valid address", Name = "Valid Name", Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Address);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi AvatarUrl vượt quá 2048 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với AvatarUrl dài hơn 2048 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính AvatarUrl với thông báo lỗi "AvatarUrl không được vượt quá 2048 ký tự.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: AvatarUrl có giới hạn độ dài tối đa là 2048 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenAvatarUrlExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), AvatarUrl = "http://" + new string('a', 2048), Name = "Valid Name", Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AvatarUrl)
              .WithErrorMessage("AvatarUrl không được vượt quá 2048 ký tự.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi AvatarUrl không phải là URL hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với AvatarUrl không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính AvatarUrl với thông báo lỗi "AvatarUrl phải là một URL hợp lệ.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: AvatarUrl phải là một URL hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenAvatarUrlIsInvalid()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), AvatarUrl = "invalid-url", Name = "Valid Name", Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AvatarUrl)
              .WithErrorMessage("AvatarUrl phải là một URL hợp lệ.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi AvatarUrl hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với AvatarUrl hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính AvatarUrl.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: AvatarUrl hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenAvatarUrlIsValid()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), AvatarUrl = "https://example.com/avatar.jpg", Name = "Valid Name", Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarUrl);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi AvatarUrl là rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với AvatarUrl là chuỗi rỗng.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính AvatarUrl.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: AvatarUrl rỗng là hợp lệ (không bắt buộc).
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenAvatarUrlIsEmpty()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), AvatarUrl = string.Empty, Name = "Valid Name", Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarUrl);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Visibility là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Visibility là null.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Visibility với thông báo lỗi "Visibility cannot be null.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Visibility là trường bắt buộc và không được để null.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenVisibilityIsNull()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Name", Visibility = null! };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Visibility)
              .WithErrorMessage("Visibility cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Visibility là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Visibility là chuỗi rỗng.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Visibility với thông báo lỗi "Visibility cannot be empty.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Visibility là trường bắt buộc và không được để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenVisibilityIsEmpty()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Name", Visibility = string.Empty };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Visibility)
              .WithErrorMessage("Visibility cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Visibility không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Visibility không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Visibility với thông báo lỗi "Visibility must be 'Public' or 'Private'.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Visibility phải là một trong các giá trị hợp lệ ('Public' hoặc 'Private').
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenVisibilityIsInvalid()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Name", Visibility = "Invalid" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Visibility)
              .WithErrorMessage("Visibility must be 'Public' or 'Private'.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Visibility hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateFamilyCommand với Visibility hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Visibility.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Visibility hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenVisibilityIsValid()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Valid Name", Visibility = "Public" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Visibility);
    }
}
