using backend.Application.Files.UploadFile;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Files.UploadFile;

public class UploadFileCommandValidatorTests
{
    private readonly UploadFileCommandValidator _validator;

    public UploadFileCommandValidatorTests()
    {
        _validator = new UploadFileCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenImageDataIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi ImageData là null.
        var command = new UploadFileCommand { ImageData = null!, FileName = "test.jpg", Folder = "test" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ImageData)
              .WithErrorMessage("Image data cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenImageDataIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi ImageData là mảng rỗng.
        var command = new UploadFileCommand { ImageData = Array.Empty<byte>(), FileName = "test.jpg", Folder = "test" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ImageData)
              .WithErrorMessage("Image data cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenImageDataIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi ImageData hợp lệ.
        var command = new UploadFileCommand { ImageData = new byte[] { 1, 2, 3 }, FileName = "test.jpg", Folder = "test" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ImageData);
    }

    [Fact]
    public void ShouldHaveError_WhenFileNameIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FileName là null.
        var command = new UploadFileCommand { ImageData = new byte[] { 1, 2, 3 }, FileName = null!, Folder = "test" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileName)
              .WithErrorMessage("File name cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenFileNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FileName là chuỗi rỗng.
        var command = new UploadFileCommand { ImageData = new byte[] { 1, 2, 3 }, FileName = string.Empty, Folder = "test" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileName)
              .WithErrorMessage("File name cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFileNameIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi FileName hợp lệ.
        var command = new UploadFileCommand { ImageData = new byte[] { 1, 2, 3 }, FileName = "test.jpg", Folder = "test" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FileName);
    }



    [Fact]
    public void ShouldHaveError_WhenFolderIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Folder là null.
        var command = new UploadFileCommand { ImageData = new byte[] { 1, 2, 3 }, FileName = "test.jpg", Folder = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Folder)
              .WithErrorMessage("Folder name cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenFolderIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Folder là chuỗi rỗng.
        var command = new UploadFileCommand { ImageData = new byte[] { 1, 2, 3 }, FileName = "test.jpg", Folder = string.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Folder)
              .WithErrorMessage("Folder name cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFolderIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Folder hợp lệ.
        var command = new UploadFileCommand { ImageData = new byte[] { 1, 2, 3 }, FileName = "test.jpg", Folder = "test" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Folder);
    }
}
