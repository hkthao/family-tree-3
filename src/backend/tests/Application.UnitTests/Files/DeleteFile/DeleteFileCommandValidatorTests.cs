using FluentValidation.TestHelper;
using Xunit;
using backend.Application.Files.DeleteFile;

namespace backend.Application.UnitTests.Files.DeleteFile;

public class DeleteFileCommandValidatorTests
{
    private readonly DeleteFileCommandValidator _validator;

    public DeleteFileCommandValidatorTests()
    {
        _validator = new DeleteFileCommandValidator();
    }

    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        var command = new DeleteFileCommand { FileId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenFileIdIsEmpty()
    {
        var command = new DeleteFileCommand { FileId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileId);
    }
}
