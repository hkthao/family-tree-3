using backend.Application.Events.Commands.CreateEvent;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandValidatorTests
{
    private readonly CreateEventCommandValidator _validator;

    public CreateEventCommandValidatorTests()
    {
        _validator = new CreateEventCommandValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Name là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với Name là null.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Name với thông báo lỗi "Name cannot be null.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name là trường bắt buộc và không được để null.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenNameIsNull()
    {
        // Arrange
        var command = new CreateEventCommand { Name = null!, FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Name là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với Name là chuỗi rỗng.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Name với thông báo lỗi "Name cannot be empty.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name là trường bắt buộc và không được để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateEventCommand { Name = string.Empty, FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Name vượt quá 200 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với Name vượt quá 200 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Name với thông báo lỗi "Name must not exceed 200 characters.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name có giới hạn độ dài tối đa là 200 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var command = new CreateEventCommand { Name = new string('a', 201), FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 200 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Name hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với Name hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Name.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Name hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenNameIsValid()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Event Name", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi FamilyId là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với FamilyId là null.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính FamilyId với thông báo lỗi "FamilyId cannot be null.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FamilyId là trường bắt buộc và không được để null.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsNull()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = null };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("FamilyId cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi FamilyId là Guid rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với FamilyId là Guid rỗng.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính FamilyId với thông báo lỗi "FamilyId cannot be empty.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FamilyId là trường bắt buộc và không được để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsEmpty()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = default(Guid) };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("FamilyId cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi FamilyId hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với FamilyId hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính FamilyId.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FamilyId hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenFamilyIdIsValid()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FamilyId);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Description vượt quá 1000 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với Description vượt quá 1000 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Description với thông báo lỗi "Description must not exceed 1000 characters.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Description có giới hạn độ dài tối đa là 1000 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), Description = new string('a', 1001) };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description must not exceed 1000 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Description hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với Description hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Description.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Description hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenDescriptionIsValid()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), Description = "Valid description" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Location vượt quá 200 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với Location vượt quá 200 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Location với thông báo lỗi "Location must not exceed 200 characters.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Location có giới hạn độ dài tối đa là 200 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenLocationExceedsMaxLength()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), Location = new string('a', 201) };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location)
              .WithErrorMessage("Location must not exceed 200 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Location hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với Location hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Location.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Location hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenLocationIsValid()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), Location = "Valid location" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Location);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Color vượt quá 20 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với Color vượt quá 20 ký tự.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Color với thông báo lỗi "Color must not exceed 20 characters.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Color có giới hạn độ dài tối đa là 20 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenColorExceedsMaxLength()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), Color = new string('a', 21) };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Color)
              .WithErrorMessage("Color must not exceed 20 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi Color hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với Color hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính Color.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Color hợp lệ không nên gây ra lỗi xác thực.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenColorIsValid()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), Color = "#FFFFFF" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Color);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi EndDate trước StartDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với EndDate trước StartDate.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính EndDate với thông báo lỗi "EndDate cannot be before StartDate.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: EndDate không được phép trước StartDate.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("EndDate cannot be before StartDate.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi EndDate sau StartDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với EndDate sau StartDate.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính EndDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: EndDate sau StartDate là hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenEndDateIsAfterStartDate()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi EndDate bằng StartDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với EndDate bằng StartDate.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính EndDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: EndDate bằng StartDate là hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenEndDateIsSameAsStartDate()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi StartDate hoặc EndDate là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo các CreateEventCommand với StartDate hoặc EndDate là null.
    ///    - Act: Gọi phương thức TestValidate của validator cho từng command.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính EndDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: StartDate hoặc EndDate có thể là null.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenStartDateOrEndDateIsNull()
    {
        // Arrange
        var command1 = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = null, EndDate = DateTime.Now };
        // Act
        var result1 = _validator.TestValidate(command1);
        // Assert
        result1.ShouldNotHaveValidationErrorFor(x => x.EndDate);

        // Arrange
        var command2 = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = null };
        // Act
        var result2 = _validator.TestValidate(command2);
        // Assert
        result2.ShouldNotHaveValidationErrorFor(x => x.EndDate);

        // Arrange
        var command3 = new CreateEventCommand { Name = "Valid Name", FamilyId = Guid.NewGuid(), StartDate = null, EndDate = null };
        // Act
        var result3 = _validator.TestValidate(command3);
        // Assert
        result3.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }
}
