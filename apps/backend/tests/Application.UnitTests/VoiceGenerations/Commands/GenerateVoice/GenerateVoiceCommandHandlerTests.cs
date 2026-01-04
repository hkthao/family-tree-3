using backend.Application.Common.Exceptions;
using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Commands.GenerateVoice;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq; // Added for mocking
using Xunit;
using backend.Application.Common.Interfaces; // Added for IVoiceAIService
using backend.Application.Voice.DTOs; // Added for VoiceGenerateResponse
using backend.Application.Common.Models; // Added for Result

namespace backend.Application.UnitTests.VoiceGenerations.Commands.GenerateVoice;

public class GenerateVoiceCommandHandlerTests : TestBase
{
    private readonly GenerateVoiceCommandHandler _handler;
    private readonly Mock<IVoiceAIService> _mockVoiceAIService; // Declared mock

    public GenerateVoiceCommandHandlerTests()
    {
        _mockVoiceAIService = new Mock<IVoiceAIService>(); // Initialized mock
        _handler = new GenerateVoiceCommandHandler(_context, _mapper, _mockVoiceAIService.Object); // Pass mock object
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
        result.Value!.AudioUrl.Should().Be("http://generated.wav"); // Changed to match mock
        result.Value!.Duration.Should().BeGreaterThan(0);

        generatedVoice.Should().NotBeNull();
        generatedVoice!.VoiceProfileId.Should().Be(voiceProfile.Id);
        generatedVoice.Text.Should().Be(command.Text);
        generatedVoice.AudioUrl.Should().Be("http://generated.wav"); // Changed to match mock
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
