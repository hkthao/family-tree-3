using FluentValidation.TestHelper;
using Xunit;
using backend.Application.Files.UploadFile;

namespace backend.Application.UnitTests.Files.UploadFile;

public class UploadFileCommandValidatorTests
{
    private readonly UploadFileCommandValidator _validator;

    public UploadFileCommandValidatorTests()
    {
        _validator = new UploadFileCommandValidator();
    }

    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        var command = new UploadFileCommand
        {
            FileStream = new MemoryStream(new byte[] { 1, 2, 3 }),
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            Length = 3
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenFileStreamIsNull()
    {
        var command = new UploadFileCommand { FileStream = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileStream);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenFileNameIsNull()
    {
        var command = new UploadFileCommand { FileName = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenFileNameIsEmpty()
    {
        var command = new UploadFileCommand { FileName = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenContentTypeIsNull()
    {
        var command = new UploadFileCommand { ContentType = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenContentTypeIsEmpty()
    {
        var command = new UploadFileCommand { ContentType = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenLengthIsZero()
    {
        var command = new UploadFileCommand { Length = 0 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Length);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenLengthIsNegative()
    {
        var command = new UploadFileCommand { Length = -1 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Length);
    }
}
