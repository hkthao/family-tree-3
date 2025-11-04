using backend.Application.AI.Chunk.ProcessFile;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.AI.Chunk.ProcessFile;

/// <summary>
/// 🎯 Mục tiêu: Kiểm thử các quy tắc xác thực của ProcessFileCommandValidator.
/// ⚙️ Các bước: Arrange - Act - Assert.
/// 💡 Giải thích: Đảm bảo validator hoạt động đúng khi các trường hợp hợp lệ và không hợp lệ.
/// </summary>
public class ProcessFileCommandValidatorTests
{
    private readonly ProcessFileCommandValidator _validator;

    public ProcessFileCommandValidatorTests()
    {
        _validator = new ProcessFileCommandValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra validator trả về lỗi khi FileStream là null.
    /// ⚙️ Arrange: Tạo ProcessFileCommand với FileStream là null.
    /// ⚙️ Act: Gọi phương thức TestValidate của validator.
    /// ⚙️ Assert: Kỳ vọng có lỗi xác thực cho FileStream với thông báo "FileStream cannot be null.".
    /// 💡 Giải thích: FileStream là trường bắt buộc và không được phép null.
    /// </summary>
    [Fact]
    public void ShouldHaveValidationError_WhenFileStreamIsNull()
    {
        // Arrange
        var command = new ProcessFileCommand { FileName = "test.txt", FileId = Guid.NewGuid().ToString(), FamilyId = Guid.NewGuid().ToString(), Category = "category", CreatedBy = "createdBy"};

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileStream)
              .WithErrorMessage("FileStream cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra validator trả về lỗi khi FileName là null hoặc rỗng.
    /// ⚙️ Arrange: Tạo ProcessFileCommand với FileName là null hoặc rỗng.
    /// ⚙️ Act: Gọi phương thức TestValidate của validator.
    /// ⚙️ Assert: Kỳ vọng có lỗi xác thực cho FileName với thông báo phù hợp.
    /// 💡 Giải thích: FileName là trường bắt buộc và không được phép null hoặc rỗng.
    /// </summary>
    [Theory]
    [InlineData(null, "FileName cannot be null.")]
    [InlineData("", "FileName cannot be empty.")]
    public void ShouldHaveValidationError_WhenFileNameIsNullOrEmpty(string fileName, string expectedErrorMessage)
    {
        // Arrange
        var command = new ProcessFileCommand { FileName = fileName, FileId = Guid.NewGuid().ToString(), FamilyId = Guid.NewGuid().ToString(), Category = "category", CreatedBy = "createdBy", FileStream = new MemoryStream() };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName)
              .WithErrorMessage(expectedErrorMessage);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra validator trả về lỗi khi FileId là null hoặc rỗng.
    /// ⚙️ Arrange: Tạo ProcessFileCommand với FileId là null hoặc rỗng.
    /// ⚙️ Act: Gọi phương thức TestValidate của validator.
    /// ⚙️ Assert: Kỳ vọng có lỗi xác thực cho FileId với thông báo phù hợp.
    /// 💡 Giải thích: FileId là trường bắt buộc và không được phép null hoặc rỗng.
    /// </summary>
    [Theory]
    [InlineData(null, "FileId cannot be empty.")]
    public void ShouldHaveValidationError_WhenFileIdIsNullOrEmpty(Guid? fileId, string expectedErrorMessage)
    {
        // Arrange
        var command = new ProcessFileCommand { FileName = "test.txt", FileId = fileId?.ToString() ?? string.Empty, FamilyId = Guid.NewGuid().ToString(), Category = "category", CreatedBy = "createdBy", FileStream = new MemoryStream() };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileId)
              .WithErrorMessage(expectedErrorMessage);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra validator trả về lỗi khi Category là null hoặc rỗng.
    /// ⚙️ Arrange: Tạo ProcessFileCommand với Category là null hoặc rỗng.
    /// ⚙️ Act: Gọi phương thức TestValidate của validator.
    /// ⚙️ Assert: Kỳ vọng có lỗi xác thực cho Category với thông báo phù hợp.
    /// 💡 Giải thích: Category là trường bắt buộc và không được phép null hoặc rỗng.
    /// </summary>
    [Theory]
    [InlineData(null, "Category cannot be null.")]
    [InlineData("", "Category cannot be empty.")]
    public void ShouldHaveValidationError_WhenCategoryIsNullOrEmpty(string category, string expectedErrorMessage)
    {
        // Arrange
        var command = new ProcessFileCommand { FileName = "test.txt", FileId = Guid.NewGuid().ToString(), FamilyId = Guid.NewGuid().ToString(), Category = category, CreatedBy = "createdBy", FileStream = new MemoryStream() };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
              .WithErrorMessage(expectedErrorMessage);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra validator trả về lỗi khi CreatedBy là null hoặc rỗng.
    /// ⚙️ Arrange: Tạo ProcessFileCommand với CreatedBy là null hoặc rỗng.
    /// ⚙️ Act: Gọi phương thức TestValidate của validator.
    /// ⚙️ Assert: Kỳ vọng có lỗi xác thực cho CreatedBy với thông báo phù hợp.
    /// 💡 Giải thích: CreatedBy là trường bắt buộc và không được phép null hoặc rỗng.
    /// </summary>
    [Theory]
    [InlineData(null, "CreatedBy cannot be null.")]
    [InlineData("", "CreatedBy cannot be empty.")]
    public void ShouldHaveValidationError_WhenCreatedByIsNullOrEmpty(string createdBy, string expectedErrorMessage)
    {
        // Arrange
        var command = new ProcessFileCommand { FileName = "test.txt", FileId = Guid.NewGuid().ToString(), FamilyId = Guid.NewGuid().ToString(), Category = "category", CreatedBy = createdBy, FileStream = new MemoryStream() };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CreatedBy)
              .WithErrorMessage(expectedErrorMessage);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra validator không trả về lỗi khi tất cả các trường hợp lệ.
    /// ⚙️ Arrange: Tạo ProcessFileCommand với tất cả các trường hợp lệ.
    /// ⚙️ Act: Gọi phương thức TestValidate của validator.
    /// ⚙️ Assert: Kỳ vọng không có lỗi xác thực.
    /// 💡 Giải thích: Khi tất cả các trường hợp lệ, validator không nên báo lỗi.
    /// </summary>
    [Fact]
    public void ShouldNotHaveValidationError_WhenCommandIsValid()
    {
        // Arrange
        var command = new ProcessFileCommand { FileName = "test.txt", FileId = Guid.NewGuid().ToString(), FamilyId = Guid.NewGuid().ToString(), Category = "category", CreatedBy = "createdBy", FileStream = new MemoryStream() };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FileStream);
        result.ShouldNotHaveValidationErrorFor(x => x.FileName);
        result.ShouldNotHaveValidationErrorFor(x => x.FileId);
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
        result.ShouldNotHaveValidationErrorFor(x => x.CreatedBy);
    }
}
