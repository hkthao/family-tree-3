using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Memories.Commands.AnalyzePhoto;
using backend.Application.Memories.DTOs;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http; // Added for IFormFile
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;
using backend.Application.UnitTests.Common; // Added for TestBase
using backend.Infrastructure.Data; // For ApplicationDbContext
using System.Threading; // Added for CancellationToken
using System.Collections.Generic; // Added for List

namespace backend.Application.UnitTests.Memories.Commands;

public class AnalyzePhotoCommandTests : TestBase // Inherit from TestBase
{
    // These are specific mocks for IOptions which will be setup per test method if needed
    private readonly Mock<IOptions<N8nSettings>> _n8nSettingsMock;
    private readonly Mock<IOptions<ImageProcessingServiceSettings>> _imageProcessingServiceSettingsMock;
    
    // We will instantiate the handler within each test method's Arrange section
    // and use a local HttpClient instance with a mocked HttpMessageHandler.

    public AnalyzePhotoCommandTests() : base() // Call base constructor
    {
        _n8nSettingsMock = new Mock<IOptions<N8nSettings>>();
        _imageProcessingServiceSettingsMock = new Mock<IOptions<ImageProcessingServiceSettings>>();
    }

    private Mock<IFormFile> CreateMockFormFile(string fileName, string contentType, byte[] content)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(content.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(content));
        return mockFile;
    }

    private AnalyzePhotoCommandHandler CreateHandler(HttpClient httpClient, N8nSettings n8nSettings, ImageProcessingServiceSettings imageProcessingServiceSettings)
    {
        // Setup options mocks for this specific handler instance
        _n8nSettingsMock.Setup(x => x.Value).Returns(n8nSettings);
        _imageProcessingServiceSettingsMock.Setup(x => x.Value).Returns(imageProcessingServiceSettings);

        return new AnalyzePhotoCommandHandler(
            _mapper, // Use _mapper from TestBase
            _context, // Use _context (in-memory) from TestBase
            _mockAuthorizationService.Object, // Use _mockAuthorizationService.Object from TestBase
            httpClient, // Use the provided HttpClient
            _n8nSettingsMock.Object,
            _imageProcessingServiceSettingsMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings(), new ImageProcessingServiceSettings { BaseUrl = "http://localhost:8000" });

        // Member is intentionally not added to the in-memory context (so FindAsync will return null naturally)

        var command = new AnalyzePhotoCommand { MemberId = memberId, File = CreateMockFormFile("test.jpg", "image/jpeg", new byte[10]).Object };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(string.Format(ErrorMessages.NotFound, $"Member with ID {memberId} not found."));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAuthorizationDenied()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync(); // Ensure Family is tracked
        var member = new Member("Last", "First", "CODE", familyId, false); // Use constructor without Family object
        member.SetId(memberId); // Set the ID manually
        
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings(), new ImageProcessingServiceSettings { BaseUrl = "http://localhost:8000" });

        // Add Family and Member to the in-memory context
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(false); // Authorization denied

        var command = new AnalyzePhotoCommand { MemberId = memberId, File = CreateMockFormFile("test.jpg", "image/jpeg", new byte[10]).Object };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.AccessDenied);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAllServicesRespondSuccessfully()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync(); // Ensure Family is tracked
        var member = new Member("Last", "First", "CODE", familyId, false); // Use constructor without Family object
        member.SetId(memberId); // Set the ID manually
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", fileContent);

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings { BaseUrl = "http://localhost:5678", PhotoAnalysisWebhook = "/webhook-test/photo-analysis" }, new ImageProcessingServiceSettings { BaseUrl = "http://localhost:8000" });

        // Add Family and Member to the in-memory context
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        // Mock Image Processing Service -> Detect Faces
        var detectFacesResponseDto = new DetectFacesResponseDto
        {
            Filename = "test.jpg",
            FaceLocations = new List<FaceLocationDto> { new FaceLocationDto { Top = 10, Right = 20, Bottom = 30, Left = 40 } }
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/detect-faces/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(detectFacesResponseDto))
            });

        // Mock Image Processing Service -> Crop and Analyze Face
        var cropAndAnalyzeFaceResponseDto = new CropAndAnalyzeFaceResponseDto
        {
            CroppedFaceBase64 = "base64encodedface",
            Emotion = "happy",
            Confidence = 0.95f,
            MemberId = memberId.ToString()
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/crop-and-analyze-face/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(cropAndAnalyzeFaceResponseDto))
            });

        // Mock N8n Webhook
        var photoAnalysisResultDto = new PhotoAnalysisResultDto
        {
            Id = Guid.NewGuid(),
            OriginalUrl = "http://example.com/image.jpg",
            Description = "A happy person in a park.",
            Scene = "Outdoor",
            Event = "Birthday",
            Emotion = "happy",
            Faces = JsonSerializer.SerializeToDocument(detectFacesResponseDto.FaceLocations),
            YearEstimate = "2020s",
            CreatedAt = DateTime.UtcNow
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/webhook-test/photo-analysis")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(photoAnalysisResultDto))
            });

        var command = new AnalyzePhotoCommand { MemberId = memberId, File = mockFile.Object };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Emotion.Should().Be("happy");
        result.Value.Description.Should().Be("A happy person in a park.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenImageProcessingServiceUrlNotConfigured()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings(), new ImageProcessingServiceSettings { BaseUrl = "" });

        var command = new AnalyzePhotoCommand { File = CreateMockFormFile("test.jpg", "image/jpeg", new byte[10]).Object };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Image Processing Service base URL is not configured.");
    }

    [Fact]
        public async Task Handle_ShouldReturnFailure_WhenDetectFacesFails()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var familyId = Guid.NewGuid();
                    var family = new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" };
                    var member = new Member("Last", "First", "CODE", familyId, false); 
                    member.SetId(memberId);            var fileContent = new byte[] { 0x01, 0x02, 0x03 };
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", fileContent);
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings(), new ImageProcessingServiceSettings { BaseUrl = "http://localhost:8000" });

        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/detect-faces/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError }); // Simulate failure

        var command = new AnalyzePhotoCommand { MemberId = memberId, File = mockFile.Object };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Internal Server Error"); // Check for generic HTTP error message
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoFacesDetected()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" };
        var member = new Member("Last", "First", "CODE", familyId, false); 
        member.SetId(memberId);
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", fileContent);

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings(), new ImageProcessingServiceSettings { BaseUrl = "http://localhost:8000" });

        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);
        var detectFacesResponseDto = new DetectFacesResponseDto
        {
            Filename = "test.jpg",
            FaceLocations = new List<FaceLocationDto>() // Empty list for no faces detected
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/detect-faces/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(detectFacesResponseDto))
            });

        var command = new AnalyzePhotoCommand { MemberId = memberId, File = mockFile.Object };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Không tìm thấy khuôn mặt nào trong ảnh.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCropAndAnalyzeFaceFails()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new Member("Last", "First", "CODE", familyId, false); 
        member.SetId(memberId);
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", fileContent);

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings(), new ImageProcessingServiceSettings { BaseUrl = "http://localhost:8000" });

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" });
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        var detectFacesResponseDto = new DetectFacesResponseDto
        {
            Filename = "test.jpg",
            FaceLocations = new List<FaceLocationDto> { new FaceLocationDto { Top = 10, Right = 20, Bottom = 30, Left = 40 } }
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/detect-faces/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(detectFacesResponseDto))
            });

        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/crop-and-analyze-face/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError }); // Simulate failure

        var command = new AnalyzePhotoCommand { MemberId = memberId, File = mockFile.Object };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Internal Server Error");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCroppedFaceBase64IsEmpty()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new Member("Last", "First", "CODE", familyId, false); 
        member.SetId(memberId);
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", fileContent);

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings(), new ImageProcessingServiceSettings { BaseUrl = "http://localhost:8000" });

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" });
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        var detectFacesResponseDto = new DetectFacesResponseDto
        {
            Filename = "test.jpg",
            FaceLocations = new List<FaceLocationDto> { new FaceLocationDto { Top = 10, Right = 20, Bottom = 30, Left = 40 } }
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/detect-faces/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(detectFacesResponseDto))
            });

        var cropAndAnalyzeFaceResponseDto = new CropAndAnalyzeFaceResponseDto
        {
            CroppedFaceBase64 = "", // Empty base64
            Emotion = "happy",
            Confidence = 0.95f,
            MemberId = memberId.ToString()
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/crop-and-analyze-face/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(cropAndAnalyzeFaceResponseDto))
            });

        var command = new AnalyzePhotoCommand { MemberId = memberId, File = mockFile.Object };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Không thể cắt và phân tích khuôn mặt.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nConfigMissing()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new Member("Last", "First", "CODE", familyId, false); 
        member.SetId(memberId);
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", fileContent);

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings { BaseUrl = "http://localhost:5678", PhotoAnalysisWebhook = "" }, new ImageProcessingServiceSettings { BaseUrl = "http://localhost:8000" });

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" });
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        var detectFacesResponseDto = new DetectFacesResponseDto
        {
            Filename = "test.jpg",
            FaceLocations = new List<FaceLocationDto> { new FaceLocationDto { Top = 10, Right = 20, Bottom = 30, Left = 40 } }
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/detect-faces/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(detectFacesResponseDto))
            });

        var cropAndAnalyzeFaceResponseDto = new CropAndAnalyzeFaceResponseDto
        {
            CroppedFaceBase64 = "base64encodedface",
            Emotion = "happy",
            Confidence = 0.95f,
            MemberId = memberId.ToString()
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/crop-and-analyze-face/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(cropAndAnalyzeFaceResponseDto))
            });

        var command = new AnalyzePhotoCommand { MemberId = memberId, File = mockFile.Object };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("N8n configuration for photo analysis is missing.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nWebhookFails()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new Member("Last", "First", "CODE", familyId, false); 
        member.SetId(memberId);
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", fileContent);

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings { BaseUrl = "http://localhost:5678", PhotoAnalysisWebhook = "/webhook-test/photo-analysis" }, new ImageProcessingServiceSettings { BaseUrl = "http://localhost:8000" });

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" });
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        var detectFacesResponseDto = new DetectFacesResponseDto
        {
            Filename = "test.jpg",
            FaceLocations = new List<FaceLocationDto> { new FaceLocationDto { Top = 10, Right = 20, Bottom = 30, Left = 40 } }
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/detect-faces/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(detectFacesResponseDto))
            });

        var cropAndAnalyzeFaceResponseDto = new CropAndAnalyzeFaceResponseDto
        {
            CroppedFaceBase64 = "base64encodedface",
            Emotion = "happy",
            Confidence = 0.95f,
            MemberId = memberId.ToString()
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/crop-and-analyze-face/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(cropAndAnalyzeFaceResponseDto))
            });

        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/webhook-test/photo-analysis")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError }); // Simulate failure

        var command = new AnalyzePhotoCommand { MemberId = memberId, File = mockFile.Object };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Internal Server Error");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nWebhookReturnsEmptyResponse()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new Member("Last", "First", "CODE", familyId, false); 
        member.SetId(memberId);
        var fileContent = new byte[] { 0x01, 0x02, 0x03 };
        var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", fileContent);

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var handler = CreateHandler(httpClient, new N8nSettings { BaseUrl = "http://localhost:5678", PhotoAnalysisWebhook = "/webhook-test/photo-analysis" }, new ImageProcessingServiceSettings { BaseUrl = "http://localhost:8000" });

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "FAMCODE" });
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(familyId)).Returns(true);

        var detectFacesResponseDto = new DetectFacesResponseDto
        {
            Filename = "test.jpg",
            FaceLocations = new List<FaceLocationDto> { new FaceLocationDto { Top = 10, Right = 20, Bottom = 30, Left = 40 } }
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/detect-faces/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(detectFacesResponseDto))
            });

        var cropAndAnalyzeFaceResponseDto = new CropAndAnalyzeFaceResponseDto
        {
            CroppedFaceBase64 = "base64encodedface",
            Emotion = "happy",
            Confidence = 0.95f,
            MemberId = memberId.ToString()
        };
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/crop-and-analyze-face/")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(cropAndAnalyzeFaceResponseDto))
            });

        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.ToString().Contains("/webhook-test/photo-analysis")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("") // Empty response
            });

        var command = new AnalyzePhotoCommand { MemberId = memberId, File = mockFile.Object };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Phân tích ảnh từ n8n trả về phản hồi rỗng hoặc không hợp lệ.");
    }
}