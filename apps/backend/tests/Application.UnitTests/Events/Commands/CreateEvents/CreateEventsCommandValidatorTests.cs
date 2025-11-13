using backend.Application.Events.Commands.CreateEvents;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvents;

public class CreateEventsCommandValidatorTests
{
    private readonly CreateEventsCommandValidator _validator;

    public CreateEventsCommandValidatorTests()
    {
        _validator = new CreateEventsCommandValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi danh sách Events rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventsCommand với danh sách Events rỗng.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Events với thông báo lỗi "Danh sách sự kiện không được để trống.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Danh sách Events là trường bắt buộc và không được để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenEventsListIsEmpty()
    {
        // Arrange
        var command = new CreateEventsCommand([]);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Events)
              .WithErrorMessage("Danh sách sự kiện không được để trống.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi danh sách Events không rỗng và hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventsCommand với danh sách Events không rỗng và hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực nào.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Danh sách Events không rỗng và hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenEventsListIsNotEmptyAndValid()
    {
        // Arrange
        var validEventDto = new CreateEventDto { Name = "Valid Event", Code = "EVT001", FamilyId = Guid.NewGuid() };
        var command = new CreateEventsCommand([validEventDto]);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi bất kỳ CreateEventDto nào trong danh sách không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventsCommand với danh sách chứa một CreateEventDto không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem tổng thể validation thất bại và có lỗi xác thực cho thuộc tính Name của CreateEventDto không hợp lệ.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Validation phải thất bại nếu bất kỳ phần tử nào trong danh sách không hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenAnyEventDtoIsInvalid()
    {
        // Arrange
        var invalidEventDto = new CreateEventDto { Name = string.Empty, Code = "EVT002", FamilyId = Guid.NewGuid() }; // Invalid name
        var validEventDto = new CreateEventDto { Name = "Valid Event", Code = "EVT001", FamilyId = Guid.NewGuid() };

        var command = new CreateEventsCommand([validEventDto, invalidEventDto]);
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.IsValid.Should().BeFalse(); // Overall validation should fail
        result.Errors.Should().Contain(e => e.PropertyName == "Events[1].Name" && e.ErrorMessage == "Tên sự kiện không được để trống.");
    }
}
