using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using backend.Application.Families.Commands.CreateFamily;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandValidatorTests
{
    private readonly CreateFamilyCommandValidator _validator;

    public CreateFamilyCommandValidatorTests()
    {
        _validator = new CreateFamilyCommandValidator();
    }

    /// <summary>
    /// Kiểm tra xem một lệnh tạo gia đình hợp lệ có không có lỗi xác thực.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi tất cả các trường bắt buộc và định dạng đều hợp lệ,
    /// validator sẽ không báo cáo bất kỳ lỗi nào.
    /// </remarks>
    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        // Arrange
        var command = new CreateFamilyCommand
        {
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
    /// Kiểm tra xem có lỗi xác thực khi trường 'Name' rỗng.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'Name' của CreateFamilyCommand là một chuỗi rỗng.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateFamilyCommand { Name = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Visibility' không hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'Visibility' của CreateFamilyCommand
    /// không phải là "Public" hoặc "Private".
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenVisibilityIsInvalid()
    {
        // Arrange
        var command = new CreateFamilyCommand { Name = "Gia đình", Visibility = "Invalid" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Visibility);
    }
}