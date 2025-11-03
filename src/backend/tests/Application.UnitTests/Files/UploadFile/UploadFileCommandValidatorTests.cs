using backend.Application.Files.UploadFile;
using FluentValidation.TestHelper;
using Xunit;
using System.IO;

namespace backend.Application.UnitTests.Files.UploadFile;

public class UploadFileCommandValidatorTests
{
    private readonly UploadFileCommandValidator _validator;

    public UploadFileCommandValidatorTests()
    {
        _validator = new UploadFileCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenFileStreamIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FileStream là null.
        var command = new UploadFileCommand { FileStream = null!, FileName = "test.txt", ContentType = "text/plain", Length = 10 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileStream)
              .WithErrorMessage("FileStream cannot be null.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFileStreamIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi FileStream hợp lệ.
        using var stream = new MemoryStream();
        var command = new UploadFileCommand { FileStream = stream, FileName = "test.txt", ContentType = "text/plain", Length = 10 };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FileStream);
    }

    [Fact]
    public void ShouldHaveError_WhenFileNameIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FileName là null.
        using var stream = new MemoryStream();
        var command = new UploadFileCommand { FileStream = stream, FileName = null!, ContentType = "text/plain", Length = 10 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileName)
              .WithErrorMessage("FileName cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenFileNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FileName là chuỗi rỗng.
        using var stream = new MemoryStream();
        var command = new UploadFileCommand { FileStream = stream, FileName = string.Empty, ContentType = "text/plain", Length = 10 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileName)
              .WithErrorMessage("FileName cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFileNameIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi FileName hợp lệ.
        using var stream = new MemoryStream();
        var command = new UploadFileCommand { FileStream = stream, FileName = "test.txt", ContentType = "text/plain", Length = 10 };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FileName);
    }

    [Fact]
    public void ShouldHaveError_WhenContentTypeIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi ContentType là null.
        using var stream = new MemoryStream();
        var command = new UploadFileCommand { FileStream = stream, FileName = "test.txt", ContentType = null!, Length = 10 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ContentType)
              .WithErrorMessage("ContentType cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenContentTypeIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi ContentType là chuỗi rỗng.
        using var stream = new MemoryStream();
        var command = new UploadFileCommand { FileStream = stream, FileName = "test.txt", ContentType = string.Empty, Length = 10 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ContentType)
              .WithErrorMessage("ContentType cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenContentTypeIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi ContentType hợp lệ.
        using var stream = new MemoryStream();
        var command = new UploadFileCommand { FileStream = stream, FileName = "test.txt", ContentType = "text/plain", Length = 10 };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ContentType);
    }

    [Fact]
    public void ShouldHaveError_WhenLengthIsZero()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Length là 0.
        using var stream = new MemoryStream();
        var command = new UploadFileCommand { FileStream = stream, FileName = "test.txt", ContentType = "text/plain", Length = 0 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Length)
              .WithErrorMessage("File length must be greater than 0.");
    }

    [Fact]
    public void ShouldHaveError_WhenLengthIsNegative()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Length là số âm.
        using var stream = new MemoryStream();
        var command = new UploadFileCommand { FileStream = stream, FileName = "test.txt", ContentType = "text/plain", Length = -1 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Length)
              .WithErrorMessage("File length must be greater than 0.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenLengthIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Length hợp lệ.
        using var stream = new MemoryStream();
        var command = new UploadFileCommand { FileStream = stream, FileName = "test.txt", ContentType = "text/plain", Length = 10 };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Length);
    }
}
