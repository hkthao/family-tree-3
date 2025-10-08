/*
using backend.Application.Common.Behaviours;
using backend.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests.Common.Behaviours;

public class LoggingBehaviourTests
{
    private readonly Mock<ILogger<TestRequest>> _mockLogger;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly LoggingBehaviour<TestRequest> _loggingBehaviour;

    public LoggingBehaviourTests()
    {
        _mockLogger = new Mock<ILogger<TestRequest>>();
        _mockUser = new Mock<IUser>();
        _mockIdentityService = new Mock<IIdentityService>();
        _loggingBehaviour = new LoggingBehaviour<TestRequest>(_mockLogger.Object, _mockUser.Object, _mockIdentityService.Object);
    }

    [Fact]
    public async Task Process_ShouldLogInformation_WhenUserIdHasValue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "testuser";
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockIdentityService.Setup(i => i.GetUserNameAsync(userId)).ReturnsAsync(userName);
        var request = new TestRequest();

        // Act
        await _loggingBehaviour.Process(request, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v!.ToString()!.Contains($"backend Request: {typeof(TestRequest).Name}") && v!.ToString()!.Contains(userId.ToString()) && v!.ToString()!.Contains(userName)),
                It.IsAny<Exception?>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Process_ShouldLogInformation_WhenUserIdIsNull()
    {
        // Arrange
        string? userId = null;
        _mockUser.Setup(u => u.Id).Returns(userId);
        var request = new TestRequest();

        // Act
        await _loggingBehaviour.Process(request, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v!.ToString()!.Contains($"backend Request: {typeof(TestRequest).Name}") && v!.ToString()!.Contains(" ") && !v!.ToString()!.Contains("testuser")), // Check for empty username and no specific user ID
                It.IsAny<Exception?>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
        _mockIdentityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }

    // Dummy request class for testing
    public class TestRequest : IRequest
    {
        public string SomeProperty { get; set; } = "Test";
    }
}
*/