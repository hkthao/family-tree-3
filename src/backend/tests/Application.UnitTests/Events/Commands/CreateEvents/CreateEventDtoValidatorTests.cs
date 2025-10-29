using backend.Application.Events.Commands.CreateEvents;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvents;

public class CreateEventDtoValidatorTests
{
    private readonly CreateEventDtoValidator _validator;

    public CreateEventDtoValidatorTests()
    {
        _validator = new CreateEventDtoValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Name là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với Name là chuỗi rỗng.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Name với thông báo lỗi "Tên sự kiện không được để trống.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name là trường bắt buộc và không được để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new CreateEventDto { Name = string.Empty, Code = "CODE", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Tên sự kiện không được để trống.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Name vượt quá 200 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với Name vượt quá 200 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Name với thông báo lỗi "Tên sự kiện không được vượt quá 200 ký tự.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name có giới hạn độ dài tối đa là 200 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var dto = new CreateEventDto { Name = new string('a', 201), Code = "CODE", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Tên sự kiện không được vượt quá 200 ký tự.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Name hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với Name hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Name.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenNameIsValid()
    {
        // Arrange
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Code là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với Code là chuỗi rỗng.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Code với thông báo lỗi "Mã sự kiện không được để trống.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Code là trường bắt buộc và không được để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenCodeIsEmpty()
    {
        // Arrange
        var dto = new CreateEventDto { Name = "Valid Name", Code = string.Empty, FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage("Mã sự kiện không được để trống.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Code vượt quá 50 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với Code vượt quá 50 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Code với thông báo lỗi "Mã sự kiện không được vượt quá 50 ký tự.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Code có giới hạn độ dài tối đa là 50 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenCodeExceedsMaxLength()
    {
        // Arrange
        var dto = new CreateEventDto { Name = "Valid Name", Code = new string('a', 51), FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage("Mã sự kiện không được vượt quá 50 ký tự.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Code hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với Code hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Code.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Code hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenCodeIsValid()
    {
        // Arrange
        var dto = new CreateEventDto { Name = "Valid Name", Code = "VALIDCODE", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi FamilyId là Guid rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với FamilyId là Guid rỗng.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính FamilyId với thông báo lỗi "ID gia đình không được để trống.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FamilyId là trường bắt buộc và không được để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsEmpty()
    {
        // Arrange
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.Empty };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("ID gia đình không được để trống.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi FamilyId hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với FamilyId hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính FamilyId.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FamilyId hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenFamilyIdIsValid()
    {
        // Arrange
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FamilyId);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi EndDate trước StartDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với EndDate trước StartDate.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính EndDate với thông báo lỗi "Ngày kết thúc không được trước ngày bắt đầu.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: EndDate không được phép trước StartDate.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("Ngày kết thúc không được trước ngày bắt đầu.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi EndDate sau StartDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với EndDate sau StartDate.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính EndDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: EndDate sau StartDate là hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenEndDateIsAfterStartDate()
    {
        // Arrange
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi EndDate bằng StartDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với EndDate bằng StartDate.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính EndDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: EndDate bằng StartDate là hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenEndDateIsSameAsStartDate()
    {
        // Arrange
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi StartDate hoặc EndDate là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo các CreateEventDto với StartDate hoặc EndDate là null.
    ///    - Act: Gọi phương thức TestValidate của validator cho từng dto.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính EndDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: StartDate hoặc EndDate có thể là null.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenStartDateOrEndDateIsNull()
    {
        // Arrange
        var dto1 = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = null, EndDate = DateTime.Now };
        // Act
        var result1 = _validator.TestValidate(dto1);
        // Assert
        result1.ShouldNotHaveValidationErrorFor(x => x.EndDate);

        // Arrange
        var dto2 = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = null };
        // Act
        var result2 = _validator.TestValidate(dto2);
        // Assert
        result2.ShouldNotHaveValidationErrorFor(x => x.EndDate);

        // Arrange
        var dto3 = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = null, EndDate = null };
        // Act
        var result3 = _validator.TestValidate(dto3);
        // Assert
        result3.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }
}
