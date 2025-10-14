using FluentValidation.TestHelper;
using Xunit;
using backend.Application.Files.CleanupUnusedFiles;

namespace backend.Application.UnitTests.Files.CleanupUnusedFiles;

public class CleanupUnusedFilesCommandValidatorTests
{
    private readonly CleanupUnusedFilesCommandValidator _validator;

    public CleanupUnusedFilesCommandValidatorTests()
    {
        _validator = new CleanupUnusedFilesCommandValidator();
    }

    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.FromMinutes(1) };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenOlderThanIsZero()
    {
        var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.Zero };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OlderThan.TotalSeconds);
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenOlderThanIsNegative()
    {
        var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.FromMinutes(-1) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OlderThan.TotalSeconds);
    }
}
