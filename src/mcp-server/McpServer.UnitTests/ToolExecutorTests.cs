using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using McpServer.Config;
using Microsoft.Extensions.Options;
using McpServer.Services.Integrations;
using McpServer.Services.Ai.AITools;
using McpServer.Models;

namespace McpServer.UnitTests;

public class ToolExecutorTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<FamilyTreeBackendService> _mockFamilyTreeBackendService;
    private readonly Mock<ILogger<ToolExecutor>> _mockLogger;
    private readonly ToolExecutor _toolExecutor;

    public ToolExecutorTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<ToolExecutor>>();

        // Mock dependencies for FamilyTreeBackendService
        var mockHttpClient = new Mock<HttpClient>();
        var mockFamilyTreeBackendSettings = new Mock<IOptions<FamilyTreeBackendSettings>>();
        var mockFamilyTreeBackendServiceLogger = new Mock<ILogger<FamilyTreeBackendService>>();

        // Create a mock for FamilyTreeBackendService using its mocked dependencies
        _mockFamilyTreeBackendService = new Mock<FamilyTreeBackendService>(
            mockHttpClient.Object,
            mockFamilyTreeBackendSettings.Object,
            mockFamilyTreeBackendServiceLogger.Object
        );

        // Setup IServiceProvider to return the mocked FamilyTreeBackendService
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(FamilyTreeBackendService)))
                            .Returns(_mockFamilyTreeBackendService.Object);
        // Setup a mock IServiceScopeFactory
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                            .Returns(mockServiceScopeFactory.Object);

        // Setup a mock IServiceScope
        var mockServiceScope = new Mock<IServiceScope>();
        mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        // Setup the IServiceScope to return the main _mockServiceProvider
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);

        _toolExecutor = new ToolExecutor(_mockServiceProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_SearchFamily_ReturnsFamilyData()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var query = "TestFamily";
        var toolCall = new AiToolCall
        {
            Id = "call_id_1",
            Function = new AiToolFunction
            {
                Name = "search_families",
                Arguments = JsonSerializer.Serialize(new { query = query })
            }
        };
        var expectedResult = new List<FamilyDto> { new FamilyDto { Id = 1, Name = query, History = "Some history" } };

        _mockFamilyTreeBackendService
            .Setup(s => s.SearchFamiliesAsync(jwtToken, query))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _toolExecutor.ExecuteAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        var actualResult = Assert.IsType<List<FamilyDto>>(result.Output);
        Assert.Single(actualResult);
        Assert.Equal(expectedResult.First().Name, actualResult.First().Name);
        _mockFamilyTreeBackendService.Verify(s => s.SearchFamiliesAsync(jwtToken, query), Times.Once);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_SearchFamily_MissingQuery_ReturnsError()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var toolCall = new AiToolCall
        {
            Id = "call_id_2",
            Function = new AiToolFunction
            {
                Name = "search_families",
                Arguments = JsonSerializer.Serialize(new { }) // Missing query
            }
        };

        // Act
        var result = await _toolExecutor.ExecuteAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains("key was not present", result.ErrorMessage);
        _mockFamilyTreeBackendService.Verify(s => s.SearchFamiliesAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_SearchFamily_NoAuth_ReturnsError()
    {
        // Arrange
        string jwtToken = string.Empty;
        var query = "TestFamily";
        var toolCall = new AiToolCall
        {
            Id = "call_id_3",
            Function = new AiToolFunction
            {
                Name = "search_families",
                Arguments = JsonSerializer.Serialize(new { query = query })
            }
        };

        _mockFamilyTreeBackendService
            .Setup(s => s.SearchFamiliesAsync(jwtToken, query))
            .ReturnsAsync((List<FamilyDto>?)null); // Return null for no auth

        // Act
        var result = await _toolExecutor.ExecuteAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains("Tool execution failed or returned no data.", result.ErrorMessage);
        _mockFamilyTreeBackendService.Verify(s => s.SearchFamiliesAsync(jwtToken, query), Times.Once);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_GetFamilyDetails_ReturnsFamilyData()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var familyId = Guid.NewGuid();
        var toolCall = new AiToolCall
        {
            Id = "call_id_4",
            Function = new AiToolFunction
            {
                Name = "get_family_details",
                Arguments = JsonSerializer.Serialize(new { familyId = familyId })
            }
        };
        var expectedResult = new FamilyDto { Id = familyId.GetHashCode(), Name = "DetailedFamily", History = "Detailed history" };

        _mockFamilyTreeBackendService
            .Setup(s => s.GetFamilyByIdAsync(familyId, jwtToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _toolExecutor.ExecuteAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        var actualResult = Assert.IsType<FamilyDto>(result.Output);
        Assert.Equal(expectedResult.Name, actualResult.Name);
        _mockFamilyTreeBackendService.Verify(s => s.GetFamilyByIdAsync(familyId, jwtToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_GetFamilyDetails_InvalidId_ReturnsError()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var toolCall = new AiToolCall
        {
            Id = "call_id_5",
            Function = new AiToolFunction
            {
                Name = "get_family_details",
                Arguments = JsonSerializer.Serialize(new { familyId = "invalid-guid" }) // Invalid GUID
            }
        };

        // Act
        var result = await _toolExecutor.ExecuteAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains("invalid", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_UnknownTool_ReturnsError()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var toolCall = new AiToolCall
        {
            Id = "call_id_6",
            Function = new AiToolFunction
            {
                Name = "unknown_tool",
                Arguments = JsonSerializer.Serialize(new { param = "value" })
            }
        };

        // Act
        var result = await _toolExecutor.ExecuteAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_InvalidJsonArgs_ReturnsError()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var toolCall = new AiToolCall
        {
            Id = "call_id_7",
            Function = new AiToolFunction
            {
                Name = "search_families",
                Arguments = "invalid json string"
            }
        };

        // Act
        var result = await _toolExecutor.ExecuteAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains("invalid start of a value", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_SearchMembers_ReturnsMemberData()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var query = "TestMember";
        var familyId = Guid.NewGuid();
        var toolCall = new AiToolCall
        {
            Id = "call_id_8",
            Function = new AiToolFunction
            {
                Name = "search_members",
                Arguments = JsonSerializer.Serialize(new { query = query, familyId = familyId })
            }
        };
        var expectedResult = new List<MemberDetailDto> { new MemberDetailDto { Id = Guid.NewGuid(), FullName = query } };

        _mockFamilyTreeBackendService
            .Setup(s => s.SearchMembersAsync(jwtToken, query, familyId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _toolExecutor.ExecuteAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        var actualResult = Assert.IsType<List<MemberDetailDto>>(result.Output);
        Assert.Single(actualResult);
        Assert.Equal(expectedResult.First().FullName, actualResult.First().FullName);
        _mockFamilyTreeBackendService.Verify(s => s.SearchMembersAsync(jwtToken, query, familyId), Times.Once);
    }
}