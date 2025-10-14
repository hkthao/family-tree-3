using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using backend.Application.Families.Commands.UpdateFamily;
using System;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandValidatorTests
{
    private readonly UpdateFamilyCommandValidator _validator;

    public UpdateFamilyCommandValidatorTests()
    {
        _validator = new UpdateFamilyCommandValidator();
    }

    /// <summary>
    /// Kiểm tra xem một lệnh cập nhật gia đình hợp lệ có không có lỗi xác thực.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi tất cả các trường bắt buộc và định dạng đều hợp lệ,
    /// validator sẽ không báo cáo bất kỳ lỗi nào.
    /// </remarks>
    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        // Arrange
        var command = new UpdateFamilyCommand
        {
            Id = Guid.NewGuid(),
            Name = "Gia đình hợp lệ",
            Description = "Mô tả hợp lệ",
            Address = "Địa chỉ hợp lệ",
            AvatarUrl = "https://example.com/avatar.jpg",
            Visibility = "Private"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Id' rỗng.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'Id' của UpdateFamilyCommand là một Guid rỗng.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenIdIsEmpty()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.Empty, Name = "Gia đình hợp lệ" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Name' rỗng.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'Name' của UpdateFamilyCommand là một chuỗi rỗng.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Visibility' không hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'Visibility' của UpdateFamilyCommand
    /// không phải là "Public" hoặc "Private".
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenVisibilityIsInvalid()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid(), Name = "Gia đình", Visibility = "Invalid" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Visibility);
    }
}