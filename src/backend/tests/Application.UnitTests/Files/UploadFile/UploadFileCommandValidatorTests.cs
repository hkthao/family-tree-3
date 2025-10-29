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

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi FileStream của UploadFileCommand là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với FileStream được đặt thành null.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính FileStream với thông báo lỗi cụ thể "FileStream cannot be null.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FileStream là một trường bắt buộc và không được phép có giá trị null để đảm bảo tệp có thể được xử lý.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenFileStreamIsNull()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileStream, (Stream)null!)
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FileStream)
            .WithErrorMessage("FileStream cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator không báo lỗi khi FileStream của UploadFileCommand được cung cấp hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với FileStream được đặt thành một MemoryStream hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng không có lỗi xác thực nào cho thuộc tính FileStream.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một FileStream hợp lệ nên vượt qua quá trình xác thực mà không có bất kỳ lỗi nào.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenFileStreamIsProvided()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.FileStream);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi FileName của UploadFileCommand là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với FileName được đặt thành null.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính FileName với thông báo lỗi cụ thể "FileName cannot be null.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FileName là một trường bắt buộc và không được phép có giá trị null để đảm bảo tệp có tên hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenFileNameIsNull()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileName, (string)null!)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FileName)
            .WithErrorMessage("FileName cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi FileName của UploadFileCommand là một chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với FileName được đặt thành string.Empty.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính FileName với thông báo lỗi cụ thể "FileName cannot be empty.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FileName là một trường bắt buộc và không được phép có giá trị rỗng để đảm bảo tệp có tên hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenFileNameIsEmpty()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileName, string.Empty)
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FileName)
            .WithErrorMessage("FileName cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator không báo lỗi khi FileName của UploadFileCommand được cung cấp hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với FileName được đặt thành một chuỗi hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng không có lỗi xác thực nào cho thuộc tính FileName.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một FileName hợp lệ nên vượt qua quá trình xác thực mà không có bất kỳ lỗi nào.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenFileNameIsProvided()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileName, _fixture.Create<string>())
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.FileName);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi ContentType của UploadFileCommand là null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với ContentType được đặt thành null.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính ContentType với thông báo lỗi cụ thể "ContentType cannot be null.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: ContentType là một trường bắt buộc và không được phép có giá trị null để đảm bảo tệp có kiểu nội dung hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenContentTypeIsNull()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.ContentType, (string)null!)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ContentType)
            .WithErrorMessage("ContentType cannot be null.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi ContentType của UploadFileCommand là một chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với ContentType được đặt thành string.Empty.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính ContentType với thông báo lỗi cụ thể "ContentType cannot be empty.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: ContentType là một trường bắt buộc và không được phép có giá trị rỗng để đảm bảo tệp có kiểu nội dung hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenContentTypeIsEmpty()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.ContentType, string.Empty)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ContentType)
            .WithErrorMessage("ContentType cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator không báo lỗi khi ContentType của UploadFileCommand được cung cấp hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với ContentType được đặt thành một chuỗi hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng không có lỗi xác thực nào cho thuộc tính ContentType.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một ContentType hợp lệ nên vượt qua quá trình xác thực mà không có bất kỳ lỗi nào.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenContentTypeIsProvided()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.ContentType, _fixture.Create<string>())
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.ContentType);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi Length của UploadFileCommand là 0.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với Length được đặt thành 0L.
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Length với thông báo lỗi cụ thể "File length must be greater than 0.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Length của tệp phải lớn hơn 0 để đảm bảo tệp không rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenLengthIsZero()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.Length, 0L)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Length)
            .WithErrorMessage("File length must be greater than 0.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi Length của UploadFileCommand là một số âm.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với Length được đặt thành một số âm (ví dụ: -1L).
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính Length với thông báo lỗi cụ thể "File length must be greater than 0.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Length của tệp phải là một số dương để đảm bảo tính hợp lệ của kích thước tệp.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenLengthIsNegative()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.Length, -1L)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Length)
            .WithErrorMessage("File length must be greater than 0.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator không báo lỗi khi Length của UploadFileCommand là một số dương.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với Length được đặt thành một số dương (ví dụ: 100L).
    ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.
    ///    - Assert: Kiểm tra rằng không có lỗi xác thực nào cho thuộc tính Length.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một Length dương hợp lệ nên vượt qua quá trình xác thực mà không có bất kỳ lỗi nào.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenLengthIsPositive()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.Length, 100L)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Length);
    }
}
