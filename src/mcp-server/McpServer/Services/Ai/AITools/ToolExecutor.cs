using System.Text.Json;
using McpServer.Models;
using McpServer.Services.Integrations;

namespace McpServer.Services.Ai.AITools;

/// <summary>
/// Executes tool calls requested by the AI model.
/// </summary>
public class ToolExecutor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ToolExecutor> _logger;

    // A dictionary mapping tool names to their execution logic.
    private readonly Dictionary<string, Func<string, string, Task<object?>>> _toolImplementations;

    public ToolExecutor(IServiceProvider serviceProvider, ILogger<ToolExecutor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _toolImplementations = new Dictionary<string, Func<string, string, Task<object?>>>
        {
            { "search_families", ExecuteSearchFamiliesAsync },
            { "get_family_details", ExecuteGetFamilyDetailsAsync },
            { "search_members", ExecuteSearchMembersAsync },
            { "get_member_details", ExecuteGetMemberDetailsAsync },
            { "search_events", ExecuteSearchEventsAsync },
            { "get_upcoming_events", ExecuteGetUpcomingEventsAsync },
        };
    }

    /// <summary>
    /// Executes a tool call and returns the result.
    /// </summary>
    /// <param name="toolCall">The tool call to execute.</param>
    /// <param name="jwtToken">The JWT token for authorization.</param>
    /// <returns>An AiToolResult representing the outcome of the execution.</returns>
    public async Task<AiToolResult> ExecuteAsync(AiToolCall toolCall, string jwtToken)
    {
        _logger.LogInformation("Executing tool: {ToolName}", toolCall.Function.Name);

        if (_toolImplementations.TryGetValue(toolCall.Function.Name, out var implementation))
        {
            try
            {
                var result = await implementation(toolCall.Function.Arguments, jwtToken);

                if (result == null)
                {
                    return new AiToolResult
                    {
                        ToolCallId = toolCall.Id,
                        ToolName = toolCall.Function.Name,
                        IsSuccess = false,
                        ErrorMessage = "Tool execution failed or returned no data."
                    };
                }

                return new AiToolResult
                {
                    ToolCallId = toolCall.Id,
                    ToolName = toolCall.Function.Name,
                    Output = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool {ToolName}", toolCall.Function.Name);
                return new AiToolResult
                {
                    ToolCallId = toolCall.Id,
                    ToolName = toolCall.Function.Name,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        return new AiToolResult
        {
            ToolCallId = toolCall.Id,
            ToolName = toolCall.Function.Name,
            IsSuccess = false,
            ErrorMessage = $"Tool '{toolCall.Function.Name}' not found."
        };
    }

    private async Task<object?> ExecuteSearchFamiliesAsync(string arguments, string jwtToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var backendService = scope.ServiceProvider.GetRequiredService<FamilyTreeBackendService>();
        var args = JsonDocument.Parse(arguments);
        var query = args.RootElement.GetProperty("query").GetString() ?? string.Empty;
        return await backendService.SearchFamiliesAsync(jwtToken, query);
    }

    private async Task<object?> ExecuteGetFamilyDetailsAsync(string arguments, string jwtToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var backendService = scope.ServiceProvider.GetRequiredService<FamilyTreeBackendService>();
        var args = JsonDocument.Parse(arguments);
        var familyId = args.RootElement.GetProperty("familyId").GetGuid();
        return await backendService.GetFamilyByIdAsync(familyId, jwtToken);
    }

    private async Task<object?> ExecuteSearchMembersAsync(string arguments, string jwtToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var backendService = scope.ServiceProvider.GetRequiredService<FamilyTreeBackendService>();
        var args = JsonDocument.Parse(arguments);
        var query = args.RootElement.GetProperty("query").GetString() ?? string.Empty;
        var familyId = args.RootElement.TryGetProperty("familyId", out var familyIdElement) ? familyIdElement.GetGuid() : (Guid?)null;
        return await backendService.SearchMembersAsync(jwtToken, query, familyId);
    }

    private async Task<object?> ExecuteGetMemberDetailsAsync(string arguments, string jwtToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var backendService = scope.ServiceProvider.GetRequiredService<FamilyTreeBackendService>();
        var args = JsonDocument.Parse(arguments);
        var memberId = args.RootElement.GetProperty("memberId").GetGuid();
        return await backendService.GetMemberByIdAsync(memberId, jwtToken);
    }

    private async Task<object?> ExecuteSearchEventsAsync(string arguments, string jwtToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var backendService = scope.ServiceProvider.GetRequiredService<FamilyTreeBackendService>();
        var args = JsonDocument.Parse(arguments);
        var query = args.RootElement.GetProperty("query").GetString() ?? string.Empty;
        var familyId = args.RootElement.TryGetProperty("familyId", out var familyIdElement) ? familyIdElement.GetGuid() : (Guid?)null;
        var startDate = args.RootElement.TryGetProperty("startDate", out var startDateElement) ? startDateElement.GetDateTime() : (DateTime?)null;
        var endDate = args.RootElement.TryGetProperty("endDate", out var endDateElement) ? endDateElement.GetDateTime() : (DateTime?)null;
        return await backendService.SearchEventsAsync(jwtToken, query, familyId, startDate, endDate);
    }

    private async Task<object?> ExecuteGetUpcomingEventsAsync(string arguments, string jwtToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var backendService = scope.ServiceProvider.GetRequiredService<FamilyTreeBackendService>();
        var args = JsonDocument.Parse(arguments);
        var familyId = args.RootElement.TryGetProperty("familyId", out var familyIdElement) ? familyIdElement.GetGuid() : (Guid?)null;
        return await backendService.GetUpcomingEventsAsync(jwtToken, familyId);
    }
}
