using backend.Application.SystemConfigurations.Commands.UpdateSystemConfiguration;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace backend.Application.UnitTests.SystemConfigurations.Commands.UpdateSystemConfiguration;

public class UpdateSystemConfigurationCommandValidatorTests
{
    private readonly UpdateSystemConfigurationCommandValidator _validator;

    public UpdateSystemConfigurationCommandValidatorTests()
    {
        _validator = new UpdateSystemConfigurationCommandValidator();
    }

    [Fact]
    public void ShouldHaveNoValidationErrors_WhenCommandIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh rằng không có lỗi xác thực khi lệnh hợp lệ.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một UpdateSystemConfigurationCommand hợp lệ.

        // 2. Act: Thực hiện xác thực trên lệnh.

        // 3. Assert: Kiểm tra rằng không có lỗi xác thực nào.

        var command = new UpdateSystemConfigurationCommand
        {
            Id = Guid.NewGuid(),
            Key = "ValidKey",
            Value = "ValidValue",
            ValueType = "string",
            Description = "Valid description."
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Lệnh với tất cả các trường hợp lệ phải vượt qua xác thực mà không có lỗi.
    }

    [Fact]
    public void ShouldHaveValidationError_WhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh rằng có lỗi xác thực khi Id trống.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một UpdateSystemConfigurationCommand với Id trống (Guid.Empty).

        // 2. Act: Thực hiện xác thực trên lệnh.

        // 3. Assert: Kiểm tra rằng có lỗi xác thực cho trường Id với thông báo lỗi chính xác.

        var command = new UpdateSystemConfigurationCommand
        {
            Id = Guid.Empty,
            Key = "ValidKey",
            Value = "ValidValue",
            ValueType = "string",
            Description = "Valid description."
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id is required.");
        // 💡 Giải thích: Id là trường bắt buộc, nên khi trống phải có lỗi xác thực.
    }

    [Fact]
    public void ShouldHaveValidationError_WhenKeyIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh rằng có lỗi xác thực khi Key trống.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một UpdateSystemConfigurationCommand với Key trống.

        // 2. Act: Thực hiện xác thực trên lệnh.

        // 3. Assert: Kiểm tra rằng có lỗi xác thực cho trường Key với thông báo lỗi chính xác.

        var command = new UpdateSystemConfigurationCommand
        {
            Id = Guid.NewGuid(),
            Key = string.Empty,
            Value = "ValidValue",
            ValueType = "string",
            Description = "Valid description."
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Key)
              .WithErrorMessage("Key is required.");
        // 💡 Giải thích: Key là trường bắt buộc, nên khi trống phải có lỗi xác thực.
    }

    [Fact]
    public void ShouldHaveValidationError_WhenKeyExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh rằng có lỗi xác thực khi Key vượt quá độ dài tối đa.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một UpdateSystemConfigurationCommand với Key dài hơn 200 ký tự.

        // 2. Act: Thực hiện xác thực trên lệnh.

        // 3. Assert: Kiểm tra rằng có lỗi xác thực cho trường Key với thông báo lỗi chính xác.

        var command = new UpdateSystemConfigurationCommand
        {
            Id = Guid.NewGuid(),
            Key = new string('a', 201),
            Value = "ValidValue",
            ValueType = "string",
            Description = "Valid description."
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Key)
              .WithErrorMessage("Key must not exceed 200 characters.");
        // 💡 Giải thích: Key không được vượt quá 200 ký tự, nên khi vượt quá phải có lỗi xác thực.
    }

    [Fact]
    public void ShouldHaveValidationError_WhenValueIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh rằng có lỗi xác thực khi Value trống.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một UpdateSystemConfigurationCommand với Value trống.

        // 2. Act: Thực hiện xác thực trên lệnh.

        // 3. Assert: Kiểm tra rằng có lỗi xác thực cho trường Value với thông báo lỗi chính xác.

        var command = new UpdateSystemConfigurationCommand
        {
            Id = Guid.NewGuid(),
            Key = "ValidKey",
            Value = string.Empty,
            ValueType = "string",
            Description = "Valid description."
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Value)
              .WithErrorMessage("Value is required.");
        // 💡 Giải thích: Value là trường bắt buộc, nên khi trống phải có lỗi xác thực.
    }

    [Fact]
    public void ShouldHaveValidationError_WhenValueTypeIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh rằng có lỗi xác thực khi ValueType trống.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một UpdateSystemConfigurationCommand với ValueType trống.

        // 2. Act: Thực hiện xác thực trên lệnh.

        // 3. Assert: Kiểm tra rằng có lỗi xác thực cho trường ValueType với thông báo lỗi chính xác.

        var command = new UpdateSystemConfigurationCommand
        {
            Id = Guid.NewGuid(),
            Key = "ValidKey",
            Value = "ValidValue",
            ValueType = string.Empty,
            Description = "Valid description."
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ValueType)
              .WithErrorMessage("ValueType is required.");
        // 💡 Giải thích: ValueType là trường bắt buộc, nên khi trống phải có lỗi xác thực.
    }

    [Fact]
    public void ShouldHaveValidationError_WhenValueTypeExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh rằng có lỗi xác thực khi ValueType vượt quá độ dài tối đa.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một UpdateSystemConfigurationCommand với ValueType dài hơn 50 ký tự.

        // 2. Act: Thực hiện xác thực trên lệnh.

        // 3. Assert: Kiểm tra rằng có lỗi xác thực cho trường ValueType với thông báo lỗi chính xác.

        var command = new UpdateSystemConfigurationCommand
        {
            Id = Guid.NewGuid(),
            Key = "ValidKey",
            Value = "ValidValue",
            ValueType = new string('a', 51),
            Description = "Valid description."
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ValueType)
              .WithErrorMessage("ValueType must not exceed 50 characters.");
        // 💡 Giải thích: ValueType không được vượt quá 50 ký tự, nên khi vượt quá phải có lỗi xác thực.
    }

    [Fact]
    public void ShouldHaveValidationError_WhenDescriptionExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh rằng có lỗi xác thực khi Description vượt quá độ dài tối đa.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một UpdateSystemConfigurationCommand với Description dài hơn 1000 ký tự.

        // 2. Act: Thực hiện xác thực trên lệnh.

        // 3. Assert: Kiểm tra rằng có lỗi xác thực cho trường Description với thông báo lỗi chính xác.

        var command = new UpdateSystemConfigurationCommand
        {
            Id = Guid.NewGuid(),
            Key = "ValidKey",
            Value = "ValidValue",
            ValueType = "string",
            Description = new string('a', 1001)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description must not exceed 1000 characters.");
        // 💡 Giải thích: Description không được vượt quá 1000 ký tự, nên khi vượt quá phải có lỗi xác thực.
    }

}