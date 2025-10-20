using backend.Application.AI.Chunk.ProcessFile;
using FluentValidation.TestHelper;
using Xunit;
using AutoFixture;
using AutoFixture.AutoMoq;
using System.IO;

namespace backend.Application.UnitTests.AI.Chunk.ProcessFile;

/// <summary>
/// Lớp kiểm thử đơn vị cho ProcessFileCommandValidator.
/// Đảm bảo rằng ProcessFileCommandValidator xác thực đúng các thuộc tính của ProcessFileCommand.
/// </summary>
public class ProcessFileCommandValidatorTests
{
    private readonly ProcessFileCommandValidator _validator; // Đối tượng validator cần kiểm thử
    private readonly Fixture _fixture; // Fixture để tạo dữ liệu giả

    /// <summary>
    /// Constructor để khởi tạo validator và fixture trước mỗi bài kiểm thử.
    /// </summary>
    public ProcessFileCommandValidatorTests()
    {
        _validator = new ProcessFileCommandValidator();
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    /// <summary>
    /// Kiểm thử kịch bản lỗi: khi FileStream là null.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenFileStreamIsNull()
    {
        // Arrange (Thiết lập)
        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.FileStream, (Stream)null!)
            .Create();

        // Act (Thực thi)
        var result = _validator.TestValidate(command);

        // Assert (Kiểm tra)
        result.ShouldHaveValidationErrorFor(x => x.FileStream)
            .WithErrorMessage("FileStream cannot be null.");
    }

    /// <summary>
    /// Kiểm thử kịch bản lỗi: khi FileName là null.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenFileNameIsNull()
    {
        // Arrange (Thiết lập)
        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.FileName, (string)null!)
            .Create();

        // Act (Thực thi)
        var result = _validator.TestValidate(command);

        // Assert (Kiểm tra)
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("FileName cannot be null.");
    }

    /// <summary>
    /// Kiểm thử kịch bản lỗi: khi FileName là chuỗi rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenFileNameIsEmpty()
    {
        // Arrange (Thiết lập)
        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.FileName, string.Empty)
            .Create();

        // Act (Thực thi)
        var result = _validator.TestValidate(command);

        // Assert (Kiểm tra)
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("FileName cannot be empty.");
    }

    /// <summary>
    /// Kiểm thử kịch bản lỗi: khi FileId là null.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenFileIdIsNull()
    {
        // Arrange (Thiết lập)
        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.FileId, (string)null!)
            .Create();

        // Act (Thực thi)
        var result = _validator.TestValidate(command);

        // Assert (Kiểm tra)
        result.ShouldHaveValidationErrorFor(x => x.FileId)
            .WithErrorMessage("FileId cannot be null.");
    }

    /// <summary>
    /// Kiểm thử kịch bản lỗi: khi FileId là chuỗi rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenFileIdIsEmpty()
    {
        // Arrange (Thiết lập)
        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.FileId, string.Empty)
            .Create();

        // Act (Thực thi)
        var result = _validator.TestValidate(command);

        // Assert (Kiểm tra)
        result.ShouldHaveValidationErrorFor(x => x.FileId)
            .WithErrorMessage("FileId cannot be empty.");
    }

    /// <summary>
    /// Kiểm thử kịch bản lỗi: khi Category là null.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenCategoryIsNull()
    {
        // Arrange (Thiết lập)
        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.Category, (string)null!)
            .Create();

        // Act (Thực thi)
        var result = _validator.TestValidate(command);

        // Assert (Kiểm tra)
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Category cannot be null.");
    }

    /// <summary>
    /// Kiểm thử kịch bản lỗi: khi Category là chuỗi rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenCategoryIsEmpty()
    {
        // Arrange (Thiết lập)
        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.Category, string.Empty)
            .Create();

        // Act (Thực thi)
        var result = _validator.TestValidate(command);

        // Assert (Kiểm tra)
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Category cannot be empty.");
    }

    /// <summary>
    /// Kiểm thử kịch bản lỗi: khi CreatedBy là null.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenCreatedByIsNull()
    {
        // Arrange (Thiết lập)
        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.CreatedBy, (string)null!)
            .Create();

        // Act (Thực thi)
        var result = _validator.TestValidate(command);

        // Assert (Kiểm tra)
        result.ShouldHaveValidationErrorFor(x => x.CreatedBy)
            .WithErrorMessage("CreatedBy cannot be null.");
    }

    /// <summary>
    /// Kiểm thử kịch bản lỗi: khi CreatedBy là chuỗi rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenCreatedByIsEmpty()
    {
        // Arrange (Thiết lập)
        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.CreatedBy, string.Empty)
            .Create();

        // Act (Thực thi)
        var result = _validator.TestValidate(command);

        // Assert (Kiểm tra)
        result.ShouldHaveValidationErrorFor(x => x.CreatedBy)
            .WithErrorMessage("CreatedBy cannot be empty.");
    }

    /// <summary>
    /// Kiểm thử kịch bản thành công: khi tất cả các trường đều hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenAllFieldsAreValid()
    {
        // Arrange (Thiết lập)
        var command = _fixture.Build<ProcessFileCommand>()
            .With(x => x.FileStream, new MemoryStream())
            .With(x => x.FileName, "test.pdf")
            .With(x => x.FileId, Guid.NewGuid().ToString())
            .With(x => x.FamilyId, Guid.NewGuid().ToString())
            .With(x => x.Category, "Documents")
            .With(x => x.CreatedBy, "user123")
            .Create();

        // Act (Thực thi)
        var result = _validator.TestValidate(command);

        // Assert (Kiểm tra)
        result.ShouldNotHaveAnyValidationErrors();
    }
}
