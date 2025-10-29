using backend.Application.Events.Commands.DeleteEvent;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.DeleteEvent;

public class DeleteEventCommandValidatorTests
{
    private readonly DeleteEventCommandValidator _validator;

    public DeleteEventCommandValidatorTests()
    {
        _validator = new DeleteEventCommandValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Id là Guid rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một DeleteEventCommand với Id là Guid rỗng.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Id với thông báo lỗi "Id cannot be empty.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Id là trường bắt buộc và không được để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        // Arrange
        var command = new DeleteEventCommand(Guid.Empty);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Id hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một DeleteEventCommand với Id hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Id.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Id hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenIdIsValid()
    {
        // Arrange
        var command = new DeleteEventCommand(Guid.NewGuid());
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
