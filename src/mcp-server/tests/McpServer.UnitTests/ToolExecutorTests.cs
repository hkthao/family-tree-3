using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using McpServer.Services;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace McpServer.UnitTests;

public class ToolExecutorTests
{
    private readonly Mock<FamilyTreeBackendService> _mockFamilyTreeBackendService;
    private readonly Mock<ILogger<ToolExecutor>> _mockLogger;
    private readonly ToolExecutor _toolExecutor;

    public ToolExecutorTests()
    {
        // Mock IConfiguration and HttpClient for FamilyTreeBackendService constructor
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["FamilyTreeBackend:BaseUrl"]).Returns("http://localhost:5000");
        var httpClient = new HttpClient();

        _mockFamilyTreeBackendService = new Mock<FamilyTreeBackendService>(httpClient, mockConfiguration.Object);
        _mockLogger = new Mock<ILogger<ToolExecutor>>();
        _toolExecutor = new ToolExecutor(_mockFamilyTreeBackendService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_SearchFamily_ReturnsFamilyData()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var query = "TestFamily";
        var toolCall = new AiToolCall(
            "call_id_1",
            "search_family",
            JsonSerializer.Serialize(new { query = query })
        );
        var expectedResult = new { families = new[] { new { id = Guid.NewGuid(), name = query } } };

        _mockFamilyTreeBackendService
            .Setup(s => s.SearchFamiliesAsync(jwtToken, query))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _toolExecutor.ExecuteToolCallAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains(query, result.Content); // Check if the content contains the query
        _mockFamilyTreeBackendService.Verify(s => s.SearchFamiliesAsync(jwtToken, query), Times.Once);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_SearchFamily_MissingQuery_ReturnsError()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var toolCall = new AiToolCall(
            "call_id_2",
            "search_family",
            JsonSerializer.Serialize(new { }) // Missing query
        );

        // Act
        var result = await _toolExecutor.ExecuteToolCallAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains("Missing 'query' argument for search_family.", result.Content);
        _mockFamilyTreeBackendService.Verify(s => s.SearchFamiliesAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_SearchFamily_NoAuth_ReturnsError()
    {
        // Arrange
        string? jwtToken = null;
        var query = "TestFamily";
        var toolCall = new AiToolCall(
            "call_id_3",
            "search_family",
            JsonSerializer.Serialize(new { query = query })
        );

        // Act
        var result = await _toolExecutor.ExecuteToolCallAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains("User is not authenticated to use tool 'search_family'.", result.Content);
        _mockFamilyTreeBackendService.Verify(s => s.SearchFamiliesAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_GetFamilyDetails_ReturnsFamilyData()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var familyId = Guid.NewGuid();
        var toolCall = new AiToolCall(
            "call_id_4",
            "get_family_details",
            JsonSerializer.Serialize(new { id = familyId })
        );
        var expectedResult = new { id = familyId, name = "DetailedFamily" };

        _mockFamilyTreeBackendService
            .Setup(s => s.GetFamilyByIdAsync(familyId, jwtToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _toolExecutor.ExecuteToolCallAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains(familyId.ToString(), result.Content);
        _mockFamilyTreeBackendService.Verify(s => s.GetFamilyByIdAsync(familyId, jwtToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_GetFamilyDetails_InvalidId_ReturnsError()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var toolCall = new AiToolCall(
            "call_id_5",
            "get_family_details",
            JsonSerializer.Serialize(new { id = "invalid-guid" }) // Invalid GUID
        );

        // Act
        var result = await _toolExecutor.ExecuteToolCallAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains("Missing or invalid 'id' argument for get_family_details.", result.Content);
        _mockFamilyTreeBackendService.Verify(s => s.GetFamilyByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_UnknownTool_ReturnsError()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var toolCall = new AiToolCall(
            "call_id_6",
            "unknown_tool",
            JsonSerializer.Serialize(new { param = "value" })
        );

        // Act
        var result = await _toolExecutor.ExecuteToolCallAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains("Unknown tool: unknown_tool", result.Content);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_InvalidJsonArgs_ReturnsError()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var toolCall = new AiToolCall(
            "call_id_7",
            "search_family",
            "invalid json string"
        );

        // Act
        var result = await _toolExecutor.ExecuteToolCallAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains("Invalid JSON arguments provided for tool 'search_family'.", result.Content);
    }
}
