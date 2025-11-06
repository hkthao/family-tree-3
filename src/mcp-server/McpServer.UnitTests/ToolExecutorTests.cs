using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using McpServer.Services;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using McpServer.Config;
using Microsoft.Extensions.Options;

namespace McpServer.UnitTests;

public class ToolExecutorTests
{
    private readonly Mock<FamilyTreeBackendService> _mockFamilyTreeBackendService;
    private readonly Mock<ILogger<ToolExecutor>> _mockLogger;
    private readonly ToolExecutor _toolExecutor;

    public ToolExecutorTests()
    {
        // Mock dependencies for FamilyTreeBackendService constructor
        var mockFamilyTreeBackendSettings = new Mock<IOptions<FamilyTreeBackendSettings>>();
        mockFamilyTreeBackendSettings.Setup(o => o.Value).Returns(new FamilyTreeBackendSettings { BaseUrl = "http://localhost:5000" });
        var mockFamilyTreeBackendServiceLogger = new Mock<ILogger<FamilyTreeBackendService>>();
        var httpClient = new HttpClient();

        _mockFamilyTreeBackendService = new Mock<FamilyTreeBackendService>(httpClient, mockFamilyTreeBackendSettings.Object, mockFamilyTreeBackendServiceLogger.Object);
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
        var expectedResult = new List<McpServer.Services.FamilyDto> { new McpServer.Services.FamilyDto { Id = 1, Name = query, History = "Some history" } };

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
        var errorDict = JsonSerializer.Deserialize<Dictionary<string, string>>(result.Content);
        Assert.NotNull(errorDict);
        Assert.True(errorDict.ContainsKey("error"));
        Assert.Contains("Missing 'query' argument for search_family.", errorDict["error"]);
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
        var errorDict = JsonSerializer.Deserialize<Dictionary<string, string>>(result.Content);
        Assert.NotNull(errorDict);
        Assert.True(errorDict.ContainsKey("error"));
        Assert.Contains("User is not authenticated to use tool 'search_family'.", errorDict["error"]);
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
        var expectedResult = new McpServer.Services.FamilyDto { Id = familyId.GetHashCode(), Name = "DetailedFamily", History = "Detailed history" };

        _mockFamilyTreeBackendService
            .Setup(s => s.GetFamilyByIdAsync(familyId, jwtToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _toolExecutor.ExecuteToolCallAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains(expectedResult.Name, result.Content); // Check for Name instead of ID
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
        var errorDict = JsonSerializer.Deserialize<Dictionary<string, string>>(result.Content);
        Assert.NotNull(errorDict);
        Assert.True(errorDict.ContainsKey("error"));
        Assert.Contains("Invalid 'id' argument for get_family_details. A valid GUID string is required.", errorDict["error"]);
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
        var errorDict = JsonSerializer.Deserialize<Dictionary<string, string>>(result.Content);
        Assert.NotNull(errorDict);
        Assert.True(errorDict.ContainsKey("error"));
        Assert.Contains("Unknown tool: unknown_tool", errorDict["error"]);
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
        var errorDict = JsonSerializer.Deserialize<Dictionary<string, string>>(result.Content);
        Assert.NotNull(errorDict);
        Assert.True(errorDict.ContainsKey("error"));
        Assert.Contains("Invalid JSON arguments provided for tool 'search_family'.", errorDict["error"]);
    }

    [Fact]
    public async Task ExecuteToolCallAsync_SearchMembers_ReturnsMemberData()
    {
        // Arrange
        var jwtToken = "test_jwt";
        var query = "TestMember";
        var familyId = Guid.NewGuid();
        var toolCall = new AiToolCall(
            "call_id_8",
            "search_members",
            JsonSerializer.Serialize(new { query = query, familyId = familyId })
        );
        var expectedResult = new List<McpServer.Services.MemberDetailDto> { new McpServer.Services.MemberDetailDto { Id = Guid.NewGuid(), FullName = query } };

        _mockFamilyTreeBackendService
            .Setup(s => s.SearchMembersAsync(jwtToken, query, familyId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _toolExecutor.ExecuteToolCallAsync(toolCall, jwtToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(toolCall.Id, result.ToolCallId);
        Assert.Contains(query, result.Content);
        _mockFamilyTreeBackendService.Verify(s => s.SearchMembersAsync(jwtToken, query, familyId), Times.Once);
    }
}