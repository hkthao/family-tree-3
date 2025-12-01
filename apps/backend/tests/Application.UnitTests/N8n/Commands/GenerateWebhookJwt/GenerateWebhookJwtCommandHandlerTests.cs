using backend.Application.Common.Interfaces;
using backend.Application.N8n.Commands.GenerateWebhookJwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Common.Models; // Thêm dòng này

namespace backend.Application.UnitTests.N8n.Commands.GenerateWebhookJwt;

public class GenerateWebhookJwtCommandHandlerTests
{
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly GenerateWebhookJwtCommandHandler _handler;
    private readonly IOptions<N8nSettings> _n8nSettings;

    public GenerateWebhookJwtCommandHandlerTests()
    {
        _mockJwtService = new Mock<IJwtService>();
        _n8nSettings = Options.Create(new N8nSettings { JwtSecret = "supersecretjwtkey" });

        _handler = new GenerateWebhookJwtCommandHandler(_mockJwtService.Object, _n8nSettings);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WithValidToken()
    {
        // Arrange
        var command = new GenerateWebhookJwtCommand { Subject = "testuser", ExpiresInMinutes = 30 };
        var expectedToken = "mocked_jwt_token";
        _mockJwtService.Setup(s => s.GenerateToken(
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<string>()
        )).Returns(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().Be(expectedToken);
        result.Value.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        _mockJwtService.Verify(s => s.GenerateToken(
            command.Subject,
            It.IsAny<DateTime>(),
            _n8nSettings.Value.JwtSecret), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenJwtSecretIsNotConfigured()
    {
        // Arrange
        var command = new GenerateWebhookJwtCommand { Subject = "testuser", ExpiresInMinutes = 30 };
        var n8nSettingsWithoutSecret = Options.Create(new N8nSettings { JwtSecret = string.Empty });
        var handler = new GenerateWebhookJwtCommandHandler(_mockJwtService.Object, n8nSettingsWithoutSecret);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("JWT Secret for n8n is not configured.");
    }
}
