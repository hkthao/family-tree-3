using System.Net;
using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.Files.Commands.UploadFileFromUrl;
using backend.Application.Files.DTOs;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace backend.Application.UnitTests.Files.Commands.UploadFileFromUrl;

public class UploadFileFromUrlCommandHandlerTests : TestBase
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly UploadFileFromUrlCommandHandler _handler;

    public UploadFileFromUrlCommandHandlerTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _handler = new UploadFileFromUrlCommandHandler(_httpClientFactoryMock.Object);
    }

    private HttpClient CreateMockHttpClient(HttpResponseMessage response)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        return new HttpClient(mockHttpMessageHandler.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenFileUploadedSuccessfully()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand
        {
            FileUrl = "http://example.com/image.jpg",
            FileName = "image.jpg",
            Folder = "family-tree-memories"
        };
        var imageData = new byte[] { 0x01, 0x02, 0x03 };
        var contentType = "image/jpeg";

        // Mock HttpClientFactory to return an HttpClient that simulates successful download
        var mockHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(imageData)
        };
        mockHttpResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        var mockHttpClient = CreateMockHttpClient(mockHttpResponse);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Url.Should().Be($"mock-url-for-{command.FileName}");
        result.Value!.Name.Should().Be(command.FileName);
        result.Value!.Filename.Should().Be(command.FileName);
        result.Value!.ContentType.Should().Be(contentType);

        _httpClientFactoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileDownloadFailsWithHttpRequestException()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand
        {
            FileUrl = "http://example.com/nonexistent.jpg",
            FileName = "nonexistent.jpg"
        };

        // Mock HttpClientFactory to return an HttpClient that throws HttpRequestException
        var mockHttpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.NotFound)); // Simulate 404
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to download file from URL");
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileDownloadFailsWithGeneralException()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand
        {
            FileUrl = "http://example.com/error.jpg",
            FileName = "error.jpg"
        };

        // Mock HttpClientFactory to return an HttpClient that simulates a general exception
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new Exception("Simulated general download error"));

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("An unexpected error occurred while downloading the file");
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
    }


}
