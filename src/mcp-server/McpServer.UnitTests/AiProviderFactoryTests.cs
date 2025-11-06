using Moq;
using McpServer.Config;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using McpServer.Services.Ai;
using McpServer.Services.Ai.Providers;
using McpServer.Services.Ai.Prompt;
namespace McpServer.UnitTests;

/// <summary>
/// Các bài kiểm tra đơn vị cho lớp AiProviderFactory.
/// </summary>
public class AiProviderFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly AiProviderFactory _aiProviderFactory;

    public AiProviderFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _aiProviderFactory = new AiProviderFactory(_mockServiceProvider.Object);
    }



    /// <summary>
    /// Kiểm tra xem GetProvider có trả về LocalLlmProvider khi yêu cầu "LocalLLM" không.
    /// </summary>
    [Fact]
    public void GetProvider_LocalLLM_ReturnsLocalLlmProvider()
    {
        // Arrange
        var mockLocalLlmSettings = new Mock<IOptions<LocalLlmSettings>>();
        mockLocalLlmSettings.Setup(o => o.Value).Returns(new LocalLlmSettings());
        var mockLocalLlmLogger = new Mock<ILogger<LocalLlmProvider>>();
        var mockHttpClient = new Mock<HttpClient>();
        var mockLocalLlmProvider = new LocalLlmProvider(mockLocalLlmSettings.Object, mockLocalLlmLogger.Object, mockHttpClient.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(LocalLlmProvider)))
                            .Returns(mockLocalLlmProvider);

        // Act
        var provider = _aiProviderFactory.GetProvider("LocalLLM");

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<LocalLlmProvider>(provider);
    }

    /// <summary>
    /// Kiểm tra xem GetProvider có ném ArgumentException khi yêu cầu nhà cung cấp không tồn tại không.
    /// </summary>
    [Fact]
    public void GetProvider_InvalidProvider_ThrowsArgumentException()
    {
        // Arrange
        var invalidProviderName = "NonExistentProvider";
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(LocalLlmProvider)))
                            .Returns((LocalLlmProvider)null!); // Ensure no provider is returned

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _aiProviderFactory.GetProvider(invalidProviderName));

        Assert.Contains($"AI provider '{invalidProviderName}' not found.", exception.Message);
    }
}
