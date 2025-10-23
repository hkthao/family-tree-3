using AutoFixture;
using backend.Application.Files.UploadFile;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Files.UploadFile;

public class UploadFileCommandValidatorTests
{
    private readonly UploadFileCommandValidator _validator;
    private readonly IFixture _fixture;

    public UploadFileCommandValidatorTests()
    {
        _validator = new UploadFileCommandValidator();
        _fixture = new Fixture();
        _fixture.Register<Stream>(() => new MemoryStream());
    }

    [Fact]
    public void ShouldHaveErrorWhenFileStreamIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi FileStream là null.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với FileStream là null.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính FileStream với thông báo phù hợp.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileStream, (Stream)null!)
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FileStream)
            .WithErrorMessage("FileStream cannot be null.");
        // 💡 Giải thích: FileStream là trường bắt buộc và không được để null.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenFileStreamIsProvided()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi FileStream được cung cấp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với FileStream hợp lệ.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi cho thuộc tính FileStream.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.FileStream);
        // 💡 Giải thích: FileStream hợp lệ không gây ra lỗi.
    }

    [Fact]
    public void ShouldHaveErrorWhenFileNameIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi FileName là null.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với FileName là null.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính FileName với thông báo phù hợp.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileName, (string)null!)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FileName)
            .WithErrorMessage("FileName cannot be null.");
        // 💡 Giải thích: FileName là trường bắt buộc và không được để null.
    }

    [Fact]
    public void ShouldHaveErrorWhenFileNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi FileName là chuỗi rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với FileName là chuỗi rỗng.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính FileName với thông báo phù hợp.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileName, string.Empty)
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FileName)
            .WithErrorMessage("FileName cannot be empty.");
        // 💡 Giải thích: FileName là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenFileNameIsProvided()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi FileName được cung cấp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với FileName hợp lệ.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi cho thuộc tính FileName.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileName, _fixture.Create<string>())
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.FileName);
        // 💡 Giải thích: FileName hợp lệ không gây ra lỗi.
    }

    [Fact]
    public void ShouldHaveErrorWhenContentTypeIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi ContentType là null.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với ContentType là null.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính ContentType với thông báo phù hợp.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.ContentType, (string)null!)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ContentType)
            .WithErrorMessage("ContentType cannot be null.");
        // 💡 Giải thích: ContentType là trường bắt buộc và không được để null.
    }

    [Fact]
    public void ShouldHaveErrorWhenContentTypeIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi ContentType là chuỗi rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với ContentType là chuỗi rỗng.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính ContentType với thông báo phù hợp.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.ContentType, string.Empty)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ContentType)
            .WithErrorMessage("ContentType cannot be empty.");
        // 💡 Giải thích: ContentType là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenContentTypeIsProvided()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi ContentType được cung cấp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với ContentType hợp lệ.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi cho thuộc tính ContentType.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.ContentType, _fixture.Create<string>())
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.ContentType);
        // 💡 Giải thích: ContentType hợp lệ không gây ra lỗi.
    }

    [Fact]
    public void ShouldHaveErrorWhenLengthIsZero()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Length là 0.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với Length là 0.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Length với thông báo phù hợp.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.Length, 0L)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Length)
            .WithErrorMessage("File length must be greater than 0.");
        // 💡 Giải thích: Length phải lớn hơn 0. 
    }

    [Fact]
    public void ShouldHaveErrorWhenLengthIsNegative()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Length là số âm.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với Length là số âm.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Length với thông báo phù hợp.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.Length, -1L)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Length)
            .WithErrorMessage("File length must be greater than 0.");
        // 💡 Giải thích: Length phải lớn hơn 0.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenLengthIsPositive()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi Length là số dương.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với Length là số dương.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi cho thuộc tính Length.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.Length, 100L)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Length);
        // 💡 Giải thích: Length dương hợp lệ không gây ra lỗi.
    }
}
