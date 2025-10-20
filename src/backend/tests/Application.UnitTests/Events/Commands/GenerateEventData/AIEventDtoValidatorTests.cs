using FluentValidation.TestHelper;
using Xunit;
using backend.Application.Events.Commands.GenerateEventData;
using backend.Application.Events.Queries;
using System;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Events.Commands.GenerateEventData;

public class AIEventDtoValidatorTests
{
    private readonly AIEventDtoValidator _validator;

    public AIEventDtoValidatorTests()
    {
        _validator = new AIEventDtoValidator();
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Name' rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        // Arrange
        var dto = new AIEventDto { Name = "" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Name' vượt quá 100 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenNameExceedsMaxLength()
    {
        // Arrange
        var dto = new AIEventDto { Name = new string('A', 101) };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Type' rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenTypeIsEmpty()
    {
        // Arrange
        var dto = new AIEventDto { Type = "" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Type' không hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenTypeIsInvalid()
    {
        // Arrange
        var dto = new AIEventDto { Type = "InvalidType" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'StartDate' rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenStartDateIsNull()
    {
        // Arrange
        var dto = new AIEventDto { StartDate = null };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi 'EndDate' trước 'StartDate'.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var dto = new AIEventDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Location' vượt quá 200 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenLocationExceedsMaxLength()
    {
        // Arrange
        var dto = new AIEventDto { Location = new string('L', 201) };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Description' vượt quá 1000 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var dto = new AIEventDto { Description = new string('D', 1001) };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'FamilyName' vượt quá 100 ký tự.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenFamilyNameExceedsMaxLength()
    {
        // Arrange
        var dto = new AIEventDto { FamilyName = new string('F', 101) };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FamilyName);
    }

    /// <summary>
    /// Kiểm tra xem không có lỗi xác thực khi AIEventDto hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldNotHaveAnyValidationErrorsWhenDtoIsValid()
    {
        // Arrange
        var dto = new AIEventDto
        {
            Name = "Valid Event Name",
            Type = EventType.Birth.ToString(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Location = "Valid Location",
            Description = "Valid Description",
            FamilyName = "Valid Family Name"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
