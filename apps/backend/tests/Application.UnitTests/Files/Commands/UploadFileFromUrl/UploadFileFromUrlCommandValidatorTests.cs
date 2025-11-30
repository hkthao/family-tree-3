using backend.Application.Files.Commands.UploadFileFromUrl;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Files.Commands.UploadFileFromUrl;

public class UploadFileFromUrlCommandValidatorTests
{
    private readonly UploadFileFromUrlCommandValidator _validator;

    public UploadFileFromUrlCommandValidatorTests()
    {
        _validator = new UploadFileFromUrlCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenFileUrlIsNull()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand { FileUrl = null!, FileName = "test.jpg", Folder = "test" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileUrl).WithErrorMessage("File URL cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenFileUrlIsEmpty()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand { FileUrl = "", FileName = "test.jpg", Folder = "test" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileUrl).WithErrorMessage("File URL cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenFileUrlIsInvalid()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand { FileUrl = "invalid-url", FileName = "test.jpg", Folder = "test" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileUrl).WithErrorMessage("File URL must be a valid URL.");
    }

    [Theory]
    [InlineData("http://valid.com/image.png")]
    [InlineData("https://valid.com/image.jpeg")]
    public void ShouldNotHaveError_WhenFileUrlIsValid(string fileUrl)
    {
        // Arrange
        var command = new UploadFileFromUrlCommand { FileUrl = fileUrl, FileName = "test.jpg", Folder = "test" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FileUrl);
    }

    [Fact]
    public void ShouldHaveError_WhenFileNameIsNull()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand { FileUrl = "http://valid.com/image.png", FileName = null!, Folder = "test" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName).WithErrorMessage("File name cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenFileNameIsEmpty()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand { FileUrl = "http://valid.com/image.png", FileName = "", Folder = "test" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName).WithErrorMessage("File name cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFileNameIsValid()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand { FileUrl = "http://valid.com/image.png", FileName = "test.jpg", Folder = "test" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FileName);
    }



    [Fact]
    public void ShouldHaveError_WhenFolderIsNull()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand { FileUrl = "http://valid.com/image.png", FileName = "test.jpg", Folder = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Folder).WithErrorMessage("Folder name cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenFolderIsEmpty()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand { FileUrl = "http://valid.com/image.png", FileName = "test.jpg", Folder = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Folder).WithErrorMessage("Folder name cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFolderIsValid()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand { FileUrl = "http://valid.com/image.png", FileName = "test.jpg", Folder = "test" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Folder);
    }
}
