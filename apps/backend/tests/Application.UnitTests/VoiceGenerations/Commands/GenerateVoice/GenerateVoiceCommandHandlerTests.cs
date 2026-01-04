using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Commands.GenerateVoice;
using backend.Application.VoiceProfiles.DTOs;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected; // Added for mocking protected members
using Xunit;

namespace backend.Application.UnitTests.VoiceGenerations.Commands.GenerateVoice;

public class GenerateVoiceCommandHandlerTests : TestBase
{
    private readonly GenerateVoiceCommandHandler _handler;
    private readonly Mock<IVoiceAIService> _mockVoiceAIService;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory; // Declared mock
    private readonly Mock<IFileStorageService> _mockFileStorageService; // Declared mock


    public GenerateVoiceCommandHandlerTests()
    {
        _mockVoiceAIService = new Mock<IVoiceAIService>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>(); // Initialized mock
        _mockFileStorageService = new Mock<IFileStorageService>(); // Initialized mock

        // Setup a mock HttpMessageHandler that returns a stream
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected() // Enable mocking of protected members
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("mock audio data")))
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(mockHttpClient);

        // Setup mock FileStorageService
        _mockFileStorageService.Setup(s => s.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FileStorageResultDto>.Success(new FileStorageResultDto { FileUrl = "http://final-permanent-url.wav" })); // Return a dummy URL

        _handler = new GenerateVoiceCommandHandler(
            _context,
            _mapper,
            _mockVoiceAIService.Object,
            _mockHttpClientFactory.Object, // Pass mock object
            _mockFileStorageService.Object); // Pass mock object
    }

    [Fact]
    public async Task Handle_ShouldGenerateVoiceAndReturnSuccess_WhenConsentIsTrue()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        _context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        var voiceProfile = new VoiceProfile(memberId, "Test Label", "http://test.wav", 10.0, 0.0, "unknown", "{}", "en", true); // Consent is true
        _context.VoiceProfiles.Add(voiceProfile);
        await _context.SaveChangesAsync();

        var command = new GenerateVoiceCommand
        {
            VoiceProfileId = voiceProfile.Id,
            Text = "Hello, this is a test."
        };

        // Configure mock VoiceAIService to return a successful result
        _mockVoiceAIService.Setup(s => s.GenerateVoiceAsync(It.IsAny<VoiceGenerateRequest>()))
            .ReturnsAsync(Result<VoiceGenerateResponse>.Success(new VoiceGenerateResponse { AudioUrl = "http://generated.wav" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var generatedVoice = await _context.VoiceGenerations.FindAsync(result.Value!.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().NotBeEmpty();
        result.Value!.VoiceProfileId.Should().Be(voiceProfile.Id);
        result.Value!.Text.Should().Be(command.Text);
        result.Value!.AudioUrl.Should().Be("http://final-permanent-url.wav"); // Changed to match mock file storage
        result.Value!.Duration.Should().BeGreaterThan(0);

        generatedVoice.Should().NotBeNull();
        generatedVoice!.VoiceProfileId.Should().Be(voiceProfile.Id);
        generatedVoice.Text.Should().Be(command.Text);
        generatedVoice.AudioUrl.Should().Be("http://final-permanent-url.wav"); // Changed to match mock file storage
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenVoiceProfileDoesNotExist()
    {
        // Arrange
        var command = new GenerateVoiceCommand
        {
            VoiceProfileId = Guid.NewGuid(),
            Text = "Hello, this is a test."
        };

        // Act & Assert
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Voice Profile not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenConsentIsFalse()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        _context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        var voiceProfile = new VoiceProfile(memberId, "Test Label", "http://test.wav", 10.0, 0.0, "unknown", "{}", "en", false); // Consent is false
        _context.VoiceProfiles.Add(voiceProfile);
        await _context.SaveChangesAsync();

        var command = new GenerateVoiceCommand
        {
            VoiceProfileId = voiceProfile.Id,
            Text = "Hello, this is a test."
        };

        // Act & Assert
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Không thể tạo giọng nói. Thành viên chưa đồng ý sử dụng hồ sơ giọng nói này.");
    }
}
