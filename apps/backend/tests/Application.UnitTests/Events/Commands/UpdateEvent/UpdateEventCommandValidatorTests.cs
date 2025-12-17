using backend.Application.Events.Commands.Inputs;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.UpdateEvent;

public class UpdateEventCommandValidatorTests
{
    private readonly UpdateEventCommandValidator _validator;

    public UpdateEventCommandValidatorTests()
    {
        _validator = new UpdateEventCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Id là Guid rỗng.
        var command = new UpdateEventCommand { Id = Guid.Empty, Name = "Valid Name", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenIdIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Id hợp lệ.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void ShouldHaveError_WhenNameIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name là null.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = null!, FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name là chuỗi rỗng.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = string.Empty, FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name vượt quá 200 ký tự.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = new string('a', 201), FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenNameIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Name hợp lệ.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Event Name", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FamilyId là null.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = null };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("FamilyId cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FamilyId là Guid rỗng.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("FamilyId cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFamilyIdIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi FamilyId hợp lệ.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FamilyId);
    }

    [Fact]
    public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Description vượt quá 1000 ký tự.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), Description = new string('a', 1001) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description must not exceed 1000 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenDescriptionIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Description hợp lệ.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), Description = "Valid description" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ShouldHaveError_WhenColorExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Color vượt quá 20 ký tự.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), Color = new string('a', 21) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Color)
              .WithErrorMessage("Color must not exceed 20 characters.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenColorIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Color hợp lệ.
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "Valid Name", FamilyId = Guid.NewGuid(), Color = "#FFFFFF" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Color);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi CalendarType không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateEventCommand với CalendarType không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính CalendarType.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: CalendarType phải là một giá trị hợp lệ của enum.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenCalendarTypeIsInvalid()
    {
        // Arrange
        var command = new UpdateEventCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Name",
            FamilyId = Guid.NewGuid(),
            CalendarType = (CalendarType)99, // Invalid enum value
            SolarDate = DateTime.Now
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CalendarType)
              .WithErrorMessage("Invalid CalendarType.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi RepeatRule không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateEventCommand với RepeatRule không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính RepeatRule.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: RepeatRule phải là một giá trị hợp lệ của enum.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenRepeatRuleIsInvalid()
    {
        // Arrange
        var command = new UpdateEventCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Name",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Solar,
            SolarDate = DateTime.Now,
            RepeatRule = (RepeatRule)99 // Invalid enum value
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RepeatRule)
              .WithErrorMessage("Invalid RepeatRule.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi sự kiện Solar không có SolarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateEventCommand với CalendarType là Solar nhưng SolarDate là null.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính SolarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Solar yêu cầu SolarDate.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenSolarEventHasNoSolarDate()
    {
        // Arrange
        var command = new UpdateEventCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Name",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Solar,
            SolarDate = null // Missing SolarDate
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SolarDate)
              .WithErrorMessage("Solar event must have a SolarDate.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi sự kiện Solar có SolarDate hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateEventCommand với CalendarType là Solar và SolarDate hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính SolarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: SolarDate hợp lệ không gây lỗi.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenSolarEventHasValidSolarDate()
    {
        // Arrange
        var command = new UpdateEventCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Name",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Solar,
            SolarDate = DateTime.Now,
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SolarDate);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi sự kiện Solar có LunarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateEventCommand với CalendarType là Solar nhưng có LunarDate.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính LunarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Solar không được có LunarDate.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenSolarEventHasLunarDate()
    {
        // Arrange
        var command = new UpdateEventCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Name",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Solar,
            SolarDate = DateTime.Now,
            LunarDate = new LunarDateInput { Day = 1, Month = 1, IsLeapMonth = false },
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LunarDate)
              .WithErrorMessage("Solar event cannot have a LunarDate.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi sự kiện Lunar không có LunarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateEventCommand với CalendarType là Lunar nhưng LunarDate là null.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính LunarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Lunar yêu cầu LunarDate.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenLunarEventHasNoLunarDate()
    {
        // Arrange
        var command = new UpdateEventCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Name",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Lunar,
            LunarDate = null // Missing LunarDate
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LunarDate)
              .WithErrorMessage("Lunar event must have a LunarDate.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh không có lỗi khi sự kiện Lunar có LunarDate hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateEventCommand với CalendarType là Lunar và LunarDate hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem không có lỗi xác thực cho thuộc tính LunarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: LunarDate hợp lệ không gây lỗi.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenLunarEventHasValidLunarDate()
    {
        // Arrange
        var command = new UpdateEventCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Name",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Lunar,
            LunarDate = new LunarDateInput { Day = 15, Month = 8, IsLeapMonth = false },
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LunarDate);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi sự kiện Lunar có SolarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateEventCommand với CalendarType là Lunar nhưng có SolarDate.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính SolarDate.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Lunar không được có SolarDate.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenLunarEventHasSolarDate()
    {
        // Arrange
        var command = new UpdateEventCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Name",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Lunar,
            SolarDate = DateTime.Now,
            LunarDate = new LunarDateInput { Day = 1, Month = 1, IsLeapMonth = false },
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SolarDate)
              .WithErrorMessage("Lunar event cannot have a SolarDate.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Day của LunarDate không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateEventCommand với CalendarType là Lunar và LunarDate có Day không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính LunarDate.Day.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Day của LunarDate phải nằm trong khoảng 1-30.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenLunarDateDayIsInvalid()
    {
        // Arrange
        var command = new UpdateEventCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Name",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Lunar,
            LunarDate = new LunarDateInput { Day = 31, Month = 1, IsLeapMonth = false }, // Invalid Day
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LunarDate!.Day)
              .WithErrorMessage("Lunar day must be between 1 and 30.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Month của LunarDate không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateEventCommand với CalendarType là Lunar và LunarDate có Month không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính LunarDate.Month.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Month của LunarDate phải nằm trong khoảng 1-12.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenLunarDateMonthIsInvalid()
    {
        // Arrange
        var command = new UpdateEventCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Name",
            FamilyId = Guid.NewGuid(),
            CalendarType = CalendarType.Lunar,
            LunarDate = new LunarDateInput { Day = 1, Month = 13, IsLeapMonth = false }, // Invalid Month
            RepeatRule = RepeatRule.None
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LunarDate!.Month)
              .WithErrorMessage("Lunar month must be between 1 and 12.");
    }
}
