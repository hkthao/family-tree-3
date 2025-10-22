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
    private readonly Mock<IDateTime> _mockDateTime;
    private readonly UploadFileCommandHandler _handler;

    public UploadFileCommandHandlerTests()
    {
        _mockFileStorage = new Mock<IFileStorage>();
        _mockConfigProvider = new Mock<IConfigProvider>();
        _mockDateTime = new Mock<IDateTime>();

        // Setup default config provider behavior
        _mockConfigProvider.Setup(c => c.GetSection<StorageSettings>())
            .Returns(new StorageSettings { MaxFileSizeMB = 5, Provider = "Local" });

        // Setup default user behavior
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());

        _handler = new UploadFileCommandHandler(
            _mockFileStorage.Object,
            _mockConfigProvider.Object,
            _context,
            _mockUser.Object,
            _mockDateTime.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi tệp rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand với Length là 0.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var command = _fixture.Build<UploadFileCommand>()
                              .With(c => c.Length, 0L)
                              .With(c => c.FileStream, new MemoryStream())
                              .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("File is empty.");
        result.ErrorSource.Should().Be("Validation");
        // 💡 Giải thích: Tệp rỗng không được phép tải lên.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileExceedsMaxSize()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi kích thước tệp vượt quá giới hạn.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _configProvider để trả về MaxFileSizeMB. Tạo UploadFileCommand với Length lớn hơn giới hạn.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
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
        // 💡 Giải thích: Tệp vượt quá kích thước tối đa không được phép tải lên.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenInvalidFileType()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi loại tệp không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo UploadFileCommand với FileName có phần mở rộng không được phép (ví dụ: ".exe").
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
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
        // 💡 Giải thích: Chỉ các loại tệp được phép mới có thể tải lên.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageUploadFails()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi quá trình tải tệp lên bộ lưu trữ thất bại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _fileStorage.UploadFileAsync() để trả về Result.Failure.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
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
        // 💡 Giải thích: Lỗi từ dịch vụ lưu trữ tệp phải được truyền lại.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileStorageUploadReturnsNullUrl()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi quá trình tải tệp lên thành công nhưng trả về URL null.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _fileStorage.UploadFileAsync() để trả về Result.Success(null).
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
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
        // 💡 Giải thích: URL tệp không được null sau khi tải lên thành công.
    }

    [Fact]
    public async Task Handle_ShouldUploadFileAndSaveMetadataSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler tải tệp lên và lưu siêu dữ liệu thành công.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UploadFileCommand hợp lệ. Mock _fileStorage.UploadFileAsync() trả về URL.
        //             Mock _user.Id và _dateTime.Now.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa URL. Xác minh các phương thức mock được gọi.
        var uploadedUrl = "http://example.com/uploaded_file.jpg";
        _mockFileStorage.Setup(fs => fs.UploadFileAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(uploadedUrl));

        var userId = Guid.NewGuid().ToString();
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
        savedMetadata.UploadedBy.Should().Be(userId);
        savedMetadata.IsActive.Should().BeTrue();
        savedMetadata.Created.Should().Be(now);
        savedMetadata.LastModified.Should().Be(now);
        // 💡 Giải thích: Handler phải tải tệp lên, lưu siêu dữ liệu vào DB và trả về URL thành công.
    }

    [Fact]
    public async Task Handle_ShouldSanitizeFileName()
    {
        // 🎯 Mục tiêu của test: Xác minh handler làm sạch tên tệp để ngăn chặn tấn công duyệt thư mục.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo UploadFileCommand với FileName chứa các ký tự không hợp lệ hoặc đường dẫn duyệt thư mục.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Xác minh _fileStorage.UploadFileAsync được gọi với tên tệp đã được làm sạch.
        var uploadedUrl = "http://example.com/sanitized_file.jpg";
        _mockFileStorage.Setup(fs => fs.UploadFileAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(uploadedUrl));

        var userId = Guid.NewGuid().ToString();
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
        // 💡 Giải thích: Tên tệp phải được làm sạch trước khi sử dụng để ngăn chặn các lỗ hổng bảo mật.
    }
}
