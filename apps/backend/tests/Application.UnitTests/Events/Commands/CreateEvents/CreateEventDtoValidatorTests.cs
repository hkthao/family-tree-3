using backend.Application.Events.Commands.CreateEvents;
using FluentValidation.TestHelper;
using Xunit;
using backend.Domain.Enums;
using backend.Application.Events.Commands.Inputs;

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
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi CalendarType không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với CalendarType không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính CalendarType.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: CalendarType phải là một giá trị hợp lệ của enum.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenCalendarTypeIsInvalid()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Name = "Valid Name",
            Code = "CODE",
            FamilyId = Guid.NewGuid(),
            CalendarType = (CalendarType)99, // Invalid enum value
            SolarDate = DateTime.Now
        };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CalendarType)
              .WithErrorMessage("Invalid CalendarType.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi RepeatRule không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với RepeatRule không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính RepeatRule.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: RepeatRule phải là một giá trị hợp lệ của enum.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenRepeatRuleIsInvalid()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Name = "Valid Name",
            Code = "CODE",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Solar,
            SolarDate = DateTime.Now,
            RepeatRule = (RepeatRule)99 // Invalid enum value
        };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RepeatRule)
              .WithErrorMessage("Invalid RepeatRule.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi sự kiện Solar không có SolarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với CalendarType là Solar nhưng SolarDate là null.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính SolarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Solar yêu cầu SolarDate.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenSolarEventHasNoSolarDate()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Name = "Valid Name",
            Code = "CODE",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Solar,
            SolarDate = null // Missing SolarDate
        };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SolarDate)
              .WithErrorMessage("Solar event must have a SolarDate.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi sự kiện Solar có SolarDate hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với CalendarType là Solar và SolarDate hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính SolarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: SolarDate hợp lệ không gây lỗi.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenSolarEventHasValidSolarDate()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Name = "Valid Name",
            Code = "CODE",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Solar,
            SolarDate = DateTime.Now,
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SolarDate);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi sự kiện Solar có LunarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với CalendarType là Solar nhưng có LunarDate.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính LunarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Solar không được có LunarDate.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenSolarEventHasLunarDate()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Name = "Valid Name",
            Code = "CODE",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Solar,
            SolarDate = DateTime.Now,
            LunarDate = new LunarDateInput { Day = 1, Month = 1, IsLeapMonth = false },
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LunarDate)
              .WithErrorMessage("Solar event cannot have a LunarDate.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi sự kiện Lunar không có LunarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với CalendarType là Lunar nhưng LunarDate là null.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính LunarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Lunar yêu cầu LunarDate.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenLunarEventHasNoLunarDate()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Name = "Valid Name",
            Code = "CODE",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Lunar,
            LunarDate = null // Missing LunarDate
        };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LunarDate)
              .WithErrorMessage("Lunar event must have a LunarDate.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi sự kiện Lunar có LunarDate hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với CalendarType là Lunar và LunarDate hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính LunarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: LunarDate hợp lệ không gây lỗi.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenLunarEventHasValidLunarDate()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Name = "Valid Name",
            Code = "CODE",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Lunar,
            LunarDate = new LunarDateInput { Day = 15, Month = 8, IsLeapMonth = false },
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LunarDate);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi sự kiện Lunar có SolarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với CalendarType là Lunar nhưng có SolarDate.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính SolarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Lunar không được có SolarDate.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenLunarEventHasSolarDate()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Name = "Valid Name",
            Code = "CODE",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Lunar,
            SolarDate = DateTime.Now,
            LunarDate = new LunarDateInput { Day = 1, Month = 1, IsLeapMonth = false },
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SolarDate)
              .WithErrorMessage("Lunar event cannot have a SolarDate.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Day của LunarDate không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với CalendarType là Lunar và LunarDate có Day không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính LunarDate.Day.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Day của LunarDate phải nằm trong khoảng 1-30.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenLunarDateDayIsInvalid()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Name = "Valid Name",
            Code = "CODE",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Lunar,
            LunarDate = new LunarDateInput { Day = 31, Month = 1, IsLeapMonth = false }, // Invalid Day
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LunarDate!.Day)
              .WithErrorMessage("Lunar day must be between 1 and 30.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Month của LunarDate không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventDto với CalendarType là Lunar và LunarDate có Month không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính LunarDate.Month.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Month của LunarDate phải nằm trong khoảng 1-12.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenLunarDateMonthIsInvalid()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Name = "Valid Name",
            Code = "CODE",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Lunar,
            LunarDate = new LunarDateInput { Day = 1, Month = 13, IsLeapMonth = false }, // Invalid Month
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(dto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LunarDate!.Month)
              .WithErrorMessage("Lunar month must be between 1 and 12.");
    }
}