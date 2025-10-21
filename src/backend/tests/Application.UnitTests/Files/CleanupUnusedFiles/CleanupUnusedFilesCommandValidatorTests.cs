using backend.Application.Files.CleanupUnusedFiles;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Files.CleanupUnusedFiles;

public class CleanupUnusedFilesCommandValidatorTests
{
    private readonly CleanupUnusedFilesCommandValidator _validator;

    public CleanupUnusedFilesCommandValidatorTests()
    {
        _validator = new CleanupUnusedFilesCommandValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenOlderThanIsNotPositive()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi OlderThan không phải là một TimeSpan dương.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CleanupUnusedFilesCommand với OlderThan là TimeSpan.Zero.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính OlderThan.TotalSeconds với thông báo phù hợp.
        var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.Zero };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.OlderThan.TotalSeconds)
            .WithErrorMessage("OlderThan must be a positive TimeSpan.");

        // Test with negative TimeSpan
        command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.FromSeconds(-1) };
        result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.OlderThan.TotalSeconds)
            .WithErrorMessage("OlderThan must be a positive TimeSpan.");
        // 💡 Giải thích: OlderThan phải là một TimeSpan dương để lệnh dọn dẹp hợp lệ.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenOlderThanIsPositive()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi OlderThan là một TimeSpan dương.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CleanupUnusedFilesCommand với OlderThan là một TimeSpan dương.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi cho thuộc tính OlderThan.TotalSeconds.
        var command = new CleanupUnusedFilesCommand { OlderThan = TimeSpan.FromDays(1) };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.OlderThan.TotalSeconds);
        // 💡 Giải thích: OlderThan là một TimeSpan dương hợp lệ, vì vậy không có lỗi nào được báo cáo.
    }
}
