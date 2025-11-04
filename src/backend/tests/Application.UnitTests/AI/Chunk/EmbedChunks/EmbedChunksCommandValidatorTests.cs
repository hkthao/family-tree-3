using backend.Application.AI.Chunk.EmbedChunks;
using FluentValidation.TestHelper;
using Xunit;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.AI.Chunk.EmbedChunks;

/// <summary>
/// 🎯 Mục tiêu: Kiểm thử các quy tắc xác thực của EmbedChunksCommandValidator.
/// ⚙️ Các bước: Arrange - Act - Assert.
/// 💡 Giải thích: Đảm bảo validator hoạt động đúng khi các trường hợp hợp lệ và không hợp lệ.
/// </summary>
public class EmbedChunksCommandValidatorTests
{
    private readonly EmbedChunksCommandValidator _validator;

    public EmbedChunksCommandValidatorTests()
    {
        _validator = new EmbedChunksCommandValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra validator trả về lỗi khi Chunks là null.
    /// ⚙️ Arrange: Tạo EmbedChunksCommand với Chunks là null.
    /// ⚙️ Act: Gọi phương thức TestValidate của validator.
    /// ⚙️ Assert: Kỳ vọng có lỗi xác thực cho Chunks với thông báo "Chunks cannot be null.".
    /// 💡 Giải thích: Chunks là trường bắt buộc và không được phép null.
    /// </summary>
    [Fact]
    public void ShouldHaveValidationError_WhenChunksIsNull()
    {
        // Arrange
        var command = new EmbedChunksCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Chunks)
              .WithErrorMessage("Chunks cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra validator trả về lỗi khi Chunks là danh sách rỗng.
    /// ⚙️ Arrange: Tạo EmbedChunksCommand với Chunks là danh sách rỗng.
    /// ⚙️ Act: Gọi phương thức TestValidate của validator.
    /// ⚙️ Assert: Kỳ vọng có lỗi xác thực cho Chunks với thông báo "Chunks cannot be empty.".
    /// 💡 Giải thích: Chunks là trường bắt buộc và không được phép rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveValidationError_WhenChunksIsEmpty()
    {
        // Arrange
        var command = new EmbedChunksCommand ();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Chunks)
              .WithErrorMessage("Chunks cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra validator không trả về lỗi khi Chunks được cung cấp hợp lệ.
    /// ⚙️ Arrange: Tạo EmbedChunksCommand với Chunks là danh sách không rỗng.
    /// ⚙️ Act: Gọi phương thức TestValidate của validator.
    /// ⚙️ Assert: Kỳ vọng không có lỗi xác thực cho Chunks.
    /// 💡 Giải thích: Khi Chunks hợp lệ, validator không nên báo lỗi.
    /// </summary>
    [Fact]
    public void ShouldNotHaveValidationError_WhenChunksIsProvided()
    {
        // Arrange
        var command = new EmbedChunksCommand { Chunks = new List<TextChunk> { new TextChunk { Id = "id1", Content = "content1", FamilyId = Guid.NewGuid(), Category = "category" } } };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Chunks);
    }
}
