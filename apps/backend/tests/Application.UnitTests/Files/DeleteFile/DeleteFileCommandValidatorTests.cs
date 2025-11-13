using backend.Application.Files.DeleteFile;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Files.DeleteFile;

public class DeleteFileCommandValidatorTests
{
    private readonly DeleteFileCommandValidator _validator;

    public DeleteFileCommandValidatorTests()
    {
        _validator = new DeleteFileCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenFileIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FileId là Guid rỗng.
        var command = new DeleteFileCommand { FileId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileId)
              .WithErrorMessage("FileId cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFileIdIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi FileId hợp lệ.
        var command = new DeleteFileCommand { FileId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FileId);
    }
}
