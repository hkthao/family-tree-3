using AutoFixture;
using backend.Application.Files.DeleteFile;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Files.DeleteFile;

public class DeleteFileCommandValidatorTests
{
    private readonly DeleteFileCommandValidator _validator;
    private readonly IFixture _fixture;

    public DeleteFileCommandValidatorTests()
    {
        _validator = new DeleteFileCommandValidator();
        _fixture = new Fixture();
    }

    [Fact]
    public void ShouldHaveErrorWhenFileIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi FileId trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một DeleteFileCommand với FileId là Guid.Empty.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính FileId với thông báo phù hợp.
        var command = new DeleteFileCommand { FileId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FileId)
            .WithErrorMessage("FileId cannot be empty.");
        // 💡 Giải thích: FileId là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenFileIdIsProvided()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi FileId được cung cấp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một DeleteFileCommand với FileId hợp lệ.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi cho thuộc tính FileId.
        var command = new DeleteFileCommand { FileId = Guid.NewGuid() };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.FileId);
        // 💡 Giải thích: FileId hợp lệ không gây ra lỗi.
    }
}
