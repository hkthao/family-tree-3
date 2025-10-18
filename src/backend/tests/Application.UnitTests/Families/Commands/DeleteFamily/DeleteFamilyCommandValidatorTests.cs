using backend.Application.Families.Commands.DeleteFamily;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandValidatorTests
{
    private readonly DeleteFamilyCommandValidator _validator;

    public DeleteFamilyCommandValidatorTests()
    {
        _validator = new DeleteFamilyCommandValidator();
    }

    /// <summary>
    /// Kiểm tra xem một lệnh xóa gia đình hợp lệ có không có lỗi xác thực.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một ID gia đình hợp lệ được cung cấp,
    /// validator sẽ không báo cáo bất kỳ lỗi nào.
    /// </remarks>
    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        // Arrange
        var command = new DeleteFamilyCommand(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Id' rỗng.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'Id' của DeleteFamilyCommand là một Guid rỗng.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenIdIsEmpty()
    {
        // Arrange
        var command = new DeleteFamilyCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
