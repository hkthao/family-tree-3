using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.Commands.UploadFileFromUrl;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace backend.Application.UnitTests.Files.Commands.UploadFileFromUrl;

public class UploadFileFromUrlCommandHandlerTests : TestBase
{
    private readonly Mock<IN8nService> _n8nServiceMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly UploadFileFromUrlCommandHandler _handler;

    public UploadFileFromUrlCommandHandlerTests()
    {
        _n8nServiceMock = new Mock<IN8nService>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _handler = new UploadFileFromUrlCommandHandler(_n8nServiceMock.Object, _httpClientFactoryMock.Object);
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
            Cloud = "imgbb",
            Folder = "family-tree-memories"
        };
        var imageData = new byte[] { 0x01, 0x02, 0x03 };
        var contentType = "image/jpeg";
        var expectedImageUrl = "http://uploaded.com/image.jpg";

        // Mock HttpClientFactory to return an HttpClient that simulates successful download
        var mockHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(imageData)
        };
        mockHttpResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        var mockHttpClient = CreateMockHttpClient(mockHttpResponse);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

        // Mock N8nService to simulate successful webhook call
        _n8nServiceMock.Setup(s => s.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = expectedImageUrl }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Url.Should().Be(expectedImageUrl);

        _httpClientFactoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Once);
        _n8nServiceMock.Verify(s => s.CallImageUploadWebhookAsync(
            It.Is<ImageUploadWebhookDto>(dto =>
                dto.ImageData.SequenceEqual(imageData) &&
                dto.FileName == command.FileName &&
                dto.Cloud == command.Cloud &&
                dto.Folder == command.Folder &&
                dto.ContentType == contentType),
            It.IsAny<CancellationToken>()
        ), Times.Once);
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
        _n8nServiceMock.Verify(s => s.CallImageUploadWebhookAsync(It.IsAny<ImageUploadWebhookDto>(), It.IsAny<CancellationToken>()), Times.Never);
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
        _n8nServiceMock.Verify(s => s.CallImageUploadWebhookAsync(It.IsAny<ImageUploadWebhookDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nWebhookCallFails()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand
        {
            FileUrl = "http://example.com/image.jpg",
            FileName = "image.jpg"
        };
        var imageData = new byte[] { 0x01, 0x02, 0x03 };
        var contentType = "image/jpeg";

        // Mock HttpClientFactory for successful download
        var mockHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(imageData)
        };
        mockHttpResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        var mockHttpClient = CreateMockHttpClient(mockHttpResponse);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

        // Mock N8nService to simulate failed webhook call
        _n8nServiceMock.Setup(s => s.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<ImageUploadResponseDto>.Failure("N8n error", ErrorSources.ExternalServiceError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("N8n error");
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _n8nServiceMock.Verify(s => s.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nWebhookReturnsNullValue()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand
        {
            FileUrl = "http://example.com/image.jpg",
            FileName = "image.jpg"
        };
        var imageData = new byte[] { 0x01, 0x02, 0x03 };
        var contentType = "image/jpeg";

        // Mock HttpClientFactory for successful download
        var mockHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(imageData)
        };
        mockHttpResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        var mockHttpClient = CreateMockHttpClient(mockHttpResponse);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

        // Mock N8nService to simulate webhook returning success with null value
        _n8nServiceMock.Setup(s => s.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = null! }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.FileUploadNullUrl);
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _n8nServiceMock.Verify(s => s.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nWebhookReturnsEmptyUrl()
    {
        // Arrange
        var command = new UploadFileFromUrlCommand
        {
            FileUrl = "http://example.com/image.jpg",
            FileName = "image.jpg"
        };
        var imageData = new byte[] { 0x01, 0x02, 0x03 };
        var contentType = "image/jpeg";

        // Mock HttpClientFactory for successful download
        var mockHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(imageData)
        };
        mockHttpResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        var mockHttpClient = CreateMockHttpClient(mockHttpResponse);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

        // Mock N8nService to simulate webhook returning success with empty URL
        _n8nServiceMock.Setup(s => s.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = "" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.FileUploadNullUrl);
        result.ErrorSource.Should().Be(ErrorSources.ExternalServiceError);
        _n8nServiceMock.Verify(s => s.CallImageUploadWebhookAsync(
            It.IsAny<ImageUploadWebhookDto>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
