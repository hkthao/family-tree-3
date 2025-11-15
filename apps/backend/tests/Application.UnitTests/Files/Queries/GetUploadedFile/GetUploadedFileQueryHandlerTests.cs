using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Files.Queries.GetUploadedFile;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Files.Queries.GetUploadedFile;

public class GetUploadedFileQueryHandlerTests : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly GetUploadedFileQueryHandler _handler;
    private readonly string _testStoragePath;

    public GetUploadedFileQueryHandlerTests()
    {
        _testStoragePath = Path.Combine(Path.GetTempPath(), "TestLocalStorage");

        if (!Directory.Exists(_testStoragePath))
        {
            Directory.CreateDirectory(_testStoragePath);
        }

        var inMemorySettings = new Dictionary<string, string?> {
            {$"{nameof(StorageSettings)}:Provider", "Local"},
            {$"{nameof(StorageSettings)}:Local:LocalStoragePath", _testStoragePath}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _handler = new GetUploadedFileQueryHandler(_configuration);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testStoragePath))
        {
            Directory.Delete(_testStoragePath, true);
        }
    }

    private async Task CreateTestFile(string fileName, string content = "test content")
    {
        var filePath = Path.Combine(_testStoragePath, fileName);
        await File.WriteAllTextAsync(filePath, content);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileDoesNotExist()
    {
        // Arrange
        var query = new GetUploadedFileQuery { FileName = "nonexistent.txt" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, "File"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Theory]
    [InlineData("test.jpg", "image/jpeg")]
    [InlineData("test.jpeg", "image/jpeg")]
    [InlineData("test.png", "image/png")]
    [InlineData("test.pdf", "application/pdf")]
    [InlineData("test.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("test.txt", "application/octet-stream")] // Default type
    public async Task Handle_ShouldReturnFileContent_WhenFileExists(string fileName, string expectedContentType)
    {
        // Arrange
        var fileContent = "Hello, world!";
        await CreateTestFile(fileName, fileContent);
        var query = new GetUploadedFileQuery { FileName = fileName };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ContentType.Should().Be(expectedContentType);
        result.Value.Content.Should().BeEquivalentTo(System.Text.Encoding.UTF8.GetBytes(fileContent));
    }

    [Fact]
    public async Task Handle_ShouldSanitizeFileName_ToPreventPathTraversal()
    {
        // Arrange
        var maliciousFileName = "../../../../etc/passwd";
        var query = new GetUploadedFileQuery { FileName = maliciousFileName };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // The handler should sanitize the filename, so File.Exists will still look in the designated folder
        // and thus not find the file, resulting in a NotFound error.
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, "File"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }
}
