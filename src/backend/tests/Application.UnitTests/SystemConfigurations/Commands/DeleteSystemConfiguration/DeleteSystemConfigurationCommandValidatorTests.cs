using System;
using backend.Application.SystemConfigurations.Commands.DeleteSystemConfiguration;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.SystemConfigurations.Commands.DeleteSystemConfiguration;

public class DeleteSystemConfigurationCommandValidatorTests
{
    private readonly DeleteSystemConfigurationCommandValidator _validator;

    public DeleteSystemConfigurationCommandValidatorTests()
    {
        _validator = new DeleteSystemConfigurationCommandValidator();
    }

    [Fact]
    public void ShouldHaveNoValidationErrors_WhenCommandIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh rằng không có lỗi xác thực khi lệnh hợp lệ.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một DeleteSystemConfigurationCommand với Id hợp lệ (không trống).

        // 2. Act: Thực hiện xác thực trên lệnh.

        // 3. Assert: Kiểm tra rằng không có lỗi xác thực nào.

        var command = new DeleteSystemConfigurationCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Lệnh với Id hợp lệ phải vượt qua xác thực mà không có lỗi.
    }

    [Fact]
    public void ShouldHaveValidationError_WhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh rằng có lỗi xác thực khi Id trống.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một DeleteSystemConfigurationCommand với Id trống (Guid.Empty).

        // 2. Act: Thực hiện xác thực trên lệnh.

        // 3. Assert: Kiểm tra rằng có lỗi xác thực cho trường Id với thông báo lỗi chính xác.

        var command = new DeleteSystemConfigurationCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id is required.");
        // 💡 Giải thích: Id là trường bắt buộc, nên khi trống phải có lỗi xác thực.
    }
}
