using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Files.UploadFile;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Files.UploadFile;

public class UploadFileCommandHandlerTests : TestBase
{
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly Mock<IConfigProvider> _mockConfigProvider;
    private readonly UploadFileCommandHandler _handler;

    public UploadFileCommandHandlerTests()
    {
        _mockFileStorage = new Mock<IFileStorage>();
        _mockConfigProvider = new Mock<IConfigProvider>();

        // Setup default config provider behavior
        _mockConfigProvider.Setup(c => c.GetSection<StorageSettings>())
            .Returns(new StorageSettings { MaxFileSizeMB = 5, Provider = "Local" });

        // Setup default user behavior
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());

        _handler = new UploadFileCommandHandler(
            _mockFileStorage.Object,
            _mockConfigProvider.Object,
            _context,
            _mockUser.Object,
            _mockDateTime.Object
        );
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi tệp được tải lên có kích thước bằng 0 (rỗng).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với thuộc tính Length được đặt thành 0L.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thất bại (IsSuccess = false) và chứa thông báo lỗi phù hợp ("File is empty.").
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Hệ thống không cho phép tải lên các tệp rỗng vì chúng không chứa dữ liệu hữu ích.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileIsEmpty()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.Length, 0L)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("File is empty.");
        result.ErrorSource.Should().Be("Validation");
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi kích thước tệp được tải lên vượt quá giới hạn tối đa cho phép.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Cấu hình _mockConfigProvider để trả về một MaxFileSizeMB cụ thể.
    ///               Tạo một UploadFileCommand với thuộc tính Length lớn hơn giới hạn này.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thất bại (IsSuccess = false) và chứa thông báo lỗi phù hợp ("File size exceeds the maximum limit...").
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Hệ thống phải từ chối các tệp vượt quá kích thước tối đa để quản lý tài nguyên và ngăn chặn lạm dụng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileExceedsMaxSize()
    {
        var maxFileSizeMB = 1; // 1 MB
        _mockConfigProvider.Setup(c => c.GetSection<StorageSettings>())
            .Returns(new StorageSettings { MaxFileSizeMB = maxFileSizeMB, Provider = "Local" });

        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.Length, (long)(maxFileSizeMB * 1024 * 1024) + 1) // 1 byte over limit
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"File size exceeds the maximum limit of {maxFileSizeMB} MB.");
        result.ErrorSource.Should().Be("Validation");
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi loại tệp được tải lên không nằm trong danh sách các loại tệp được phép.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với thuộc tính FileName có phần mở rộng không được phép (ví dụ: ".exe").
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thất bại (IsSuccess = false) và chứa thông báo lỗi phù hợp ("Invalid file type...").
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Hệ thống phải từ chối các tệp có loại không an toàn hoặc không mong muốn để duy trì bảo mật và tính toàn vẹn.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenInvalidFileType()
    {
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileName, "malicious.exe")
                              .With(c => c.Length, 100L)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid file type. Only JPG, JPEG, PNG, PDF, DOCX are allowed.");
        result.ErrorSource.Should().Be("Validation");
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi dịch vụ lưu trữ tệp (IFileStorage) không thể tải lên tệp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Cấu hình _mockFileStorage.UploadFileAsync để trả về một Result.Failure.
    ///               Tạo một UploadFileCommand hợp lệ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thất bại (IsSuccess = false) và chứa thông báo lỗi phù hợp ("Storage error.").
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải truyền lại lỗi từ dịch vụ lưu trữ tệp để thông báo cho người dùng về sự cố tải lên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageUploadFails()
    {
        _mockFileStorage.Setup(fs => fs.UploadFileAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("Storage error.", "FileStorage"));

        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileName, "valid.jpg")
                              .With(c => c.Length, 100L)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Storage error.");
        result.ErrorSource.Should().Be("FileStorage");
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi dịch vụ lưu trữ tệp (IFileStorage) báo cáo tải lên thành công nhưng trả về một URL null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Cấu hình _mockFileStorage.UploadFileAsync để trả về Result.Success(null).
    ///               Tạo một UploadFileCommand hợp lệ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thất bại (IsSuccess = false) và chứa thông báo lỗi phù hợp ("File upload succeeded but returned a null URL.").
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một URL hợp lệ là cần thiết sau khi tải lên thành công để có thể truy cập tệp. URL null cho thấy có vấn đề trong quá trình lưu trữ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageUploadReturnsNullUrl()
    {
        _mockFileStorage.Setup(fs => fs.UploadFileAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success((string)null!));

        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileName, "valid.png")
                              .With(c => c.Length, 100L)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("File upload succeeded but returned a null URL.");
        result.ErrorSource.Should().Be("FileStorage");
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tải tệp lên dịch vụ lưu trữ và lưu siêu dữ liệu của tệp vào cơ sở dữ liệu thành công.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Cấu hình _mockFileStorage.UploadFileAsync để trả về một URL thành công.
    ///               Cấu hình _mockUser.Id và _mockDateTime.Now.
    ///               Tạo một UploadFileCommand hợp lệ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra rằng kết quả trả về là thành công (IsSuccess = true) và chứa URL của tệp đã tải lên.
    ///              Xác minh rằng _mockFileStorage.UploadFileAsync được gọi một lần.
    ///              Xác minh rằng một bản ghi FileMetadata mới đã được thêm vào cơ sở dữ liệu với các thuộc tính chính xác.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Đây là kịch bản thành công chính, nơi tệp được tải lên và thông tin của nó được lưu trữ đúng cách.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUploadFileAndSaveMetadataSuccessfully()
    {
        var uploadedUrl = "http://example.com/uploaded_file.jpg";
        _mockFileStorage.Setup(fs => fs.UploadFileAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(uploadedUrl));

        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var now = DateTime.UtcNow;
        _mockDateTime.Setup(dt => dt.Now).Returns(now);

        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileName, "image.jpg")
                              .With(c => c.ContentType, "image/jpeg")
                              .With(c => c.Length, 1024L)
                              .With(c => c.FileStream, new MemoryStream(new byte[1024]))
                              .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(uploadedUrl);

        _mockFileStorage.Verify(fs => fs.UploadFileAsync(
            It.IsAny<Stream>(),
            It.Is<string>(s => s.Contains(Path.GetFileNameWithoutExtension(command.FileName)) && s.EndsWith(".jpg")),
            command.ContentType,
            It.IsAny<CancellationToken>()), Times.Once);

        _context.FileMetadata.Should().ContainSingle();
        var savedMetadata = _context.FileMetadata.First();
        savedMetadata.FileName.Should().StartWith(Path.GetFileNameWithoutExtension(command.FileName));
        savedMetadata.FileName.Should().EndWith(".jpg");
        savedMetadata.Url.Should().Be(uploadedUrl);
        savedMetadata.StorageProvider.Should().Be(StorageProvider.Local);
        savedMetadata.ContentType.Should().Be(command.ContentType);
        savedMetadata.FileSize.Should().Be(command.Length);
        savedMetadata.UploadedBy.Should().Be(userId.ToString());
        savedMetadata.IsDeleted.Should().BeFalse();
        savedMetadata.Created.Should().Be(now);
        savedMetadata.LastModified.Should().Be(now);
    }

        /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler làm sạch tên tệp để ngăn chặn các cuộc tấn công duyệt thư mục
    /// và các ký tự không hợp lệ trước khi tải lên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UploadFileCommand với FileName chứa các ký tự không hợp lệ hoặc các chuỗi duyệt thư mục (ví dụ: "../../../evil_script.jpg").
    ///               Cấu hình _mockFileStorage.UploadFileAsync để trả về một URL thành công.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra rằng _mockFileStorage.UploadFileAsync được gọi với một tên tệp đã được làm sạch, không chứa các chuỗi duyệt thư mục và các ký tự không hợp lệ.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Việc làm sạch tên tệp là rất quan trọng để ngăn chặn các lỗ hổng bảo mật và đảm bảo tính toàn vẹn của hệ thống tệp.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldSanitizeFileName()
    {
        var uploadedUrl = "http://example.com/sanitized_file.jpg";
        _mockFileStorage.Setup(fs => fs.UploadFileAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(uploadedUrl));

        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var now = DateTime.UtcNow;
        _mockDateTime.Setup(dt => dt.Now).Returns(now);

        var maliciousFileName = "../../../evil_script.jpg";
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.FileName, maliciousFileName)
                              .With(c => c.ContentType, "image/jpeg")
                              .With(c => c.Length, 1024L)
                              .With(c => c.FileStream, new MemoryStream(new byte[1024]))
                              .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockFileStorage.Verify(fs => fs.UploadFileAsync(
            It.IsAny<Stream>(),
            It.Is<string>(s => s.Contains("evil_script") && !s.Contains("..") && s.EndsWith(".jpg")),
            command.ContentType,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
