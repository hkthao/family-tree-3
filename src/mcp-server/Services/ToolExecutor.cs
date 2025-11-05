using System.Text.Json;

namespace McpServer.Services;

/// <summary>
/// Thực thi các lời gọi tool từ AI, bao gồm xác thực đối số và gọi backend service.
/// </summary>
public class ToolExecutor
{
    private readonly FamilyTreeBackendService _familyTreeBackendService;
    private readonly ILogger<ToolExecutor> _logger;

    public ToolExecutor(FamilyTreeBackendService familyTreeBackendService, ILogger<ToolExecutor> logger)
    {
        _familyTreeBackendService = familyTreeBackendService;
        _logger = logger;
    }

    /// <summary>
    /// Thực thi một lời gọi tool cụ thể.
    /// </summary>
    /// <param name="toolCall">Thông tin về lời gọi tool.</param>
    /// <param name="jwtToken">JWT token của người dùng để xác thực các cuộc gọi backend.</param>
    /// <returns>Kết quả của lời gọi tool dưới dạng AiToolResult.</returns>
    public async Task<AiToolResult> ExecuteToolCallAsync(AiToolCall toolCall, string? jwtToken)
    {
        _logger.LogInformation("Executing tool: {FunctionName}", toolCall.FunctionName);
        object? result = null;
                    JsonElement args;
                    try
                    {
                        // Use JsonDocument.Parse to handle potential trailing characters or malformed JSON more robustly
                        // and then get the RootElement. Trim the string to remove any leading/trailing whitespace.
                        using (JsonDocument doc = JsonDocument.Parse(toolCall.FunctionArgs.Trim()))
                        {
                            args = doc.RootElement.Clone();
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize function arguments for tool '{FunctionName}': {FunctionArgs}. Invalid JSON. Details: {ErrorMessage}", toolCall.FunctionName, toolCall.FunctionArgs, ex.Message);
                        result = new { error = $"Invalid JSON arguments provided for tool '{toolCall.FunctionName}'. Details: {ex.Message}" };
                        return new AiToolResult(toolCall.Id, JsonSerializer.Serialize(result));
                    }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while parsing function arguments for tool '{FunctionName}': {FunctionArgs}", toolCall.FunctionName, toolCall.FunctionArgs);
            result = new { error = $"An unexpected error occurred while parsing arguments for tool '{toolCall.FunctionName}'. Details: {ex.Message}" };
            return new AiToolResult(toolCall.Id, JsonSerializer.Serialize(result));
        }

        // Validate authentication for tools that require it
        if (string.IsNullOrEmpty(jwtToken) && RequiresAuthentication(toolCall.FunctionName))
        {
            result = new { error = $"User is not authenticated to use tool '{toolCall.FunctionName}'." };
            return new AiToolResult(toolCall.Id, JsonSerializer.Serialize(result));
        }

        switch (toolCall.FunctionName)
        {
            case "search_family":
                var querySearchFamily = args.TryGetProperty("query", out var qsf) ? qsf.GetString() : string.Empty;
                if (string.IsNullOrEmpty(querySearchFamily))
                {
                    result = new { error = "Missing 'query' argument for search_family." };
                }
                else
                {
                    result = await _familyTreeBackendService.SearchFamiliesAsync(jwtToken!, querySearchFamily);
                }
                break;
            case "get_family_details":
                var idGetFamilyDetails = args.TryGetProperty("id", out var igfd) ? igfd.GetGuid() : Guid.Empty;
                if (idGetFamilyDetails == Guid.Empty)
                {
                    result = new { error = "Missing or invalid 'id' argument for get_family_details. A valid GUID is required." };
                }
                else
                {
                    result = await _familyTreeBackendService.GetFamilyByIdAsync(idGetFamilyDetails, jwtToken!);
                }
                break;
            case "search_members":
                var querySearchMembers = args.TryGetProperty("query", out var qsm) ? qsm.GetString() : string.Empty;
                var familyIdSearchMembers = args.TryGetProperty("familyId", out var fism) ? fism.GetGuid() : (Guid?)null;
                if (string.IsNullOrEmpty(querySearchMembers))
                {
                    result = new { error = "Missing 'query' argument for search_members." };
                }
                else
                {
                    result = await _familyTreeBackendService.SearchMembersAsync(jwtToken!, querySearchMembers, familyIdSearchMembers);
                }
                break;
            case "get_member_details":
                var idGetMemberDetails = args.TryGetProperty("id", out var igmd) ? igmd.GetGuid() : Guid.Empty;
                if (idGetMemberDetails == Guid.Empty)
                {
                    result = new { error = "Missing or invalid 'id' argument for get_member_details. A valid GUID is required." };
                }
                else
                {
                    result = await _familyTreeBackendService.GetMemberByIdAsync(idGetMemberDetails, jwtToken!);
                }
                break;
            case "search_events":
                var querySearchEvents = args.TryGetProperty("query", out var qse) ? qse.GetString() : string.Empty;
                var familyIdSearchEvents = args.TryGetProperty("familyId", out var fise) ? fise.GetGuid() : (Guid?)null;
                var startDateSearchEvents = args.TryGetProperty("startDate", out var sdese) ? sdese.GetDateTime() : (DateTime?)null;
                var endDateSearchEvents = args.TryGetProperty("endDate", out var edese) ? edese.GetDateTime() : (DateTime?)null;
                if (string.IsNullOrEmpty(querySearchEvents))
                {
                    result = new { error = "Missing 'query' argument for search_events." };
                }
                else
                {
                    result = await _familyTreeBackendService.SearchEventsAsync(jwtToken!, querySearchEvents, familyIdSearchEvents, startDateSearchEvents, endDateSearchEvents);
                }
                break;
            case "get_upcoming_events":
                var familyIdGetUpcomingEvents = args.TryGetProperty("familyId", out var figue) ? figue.GetGuid() : (Guid?)null;
                result = await _familyTreeBackendService.GetUpcomingEventsAsync(jwtToken!, familyIdGetUpcomingEvents);
                break;
            default:
                result = new { error = $"Unknown tool: {toolCall.FunctionName}" };
                break;
        }

        return new AiToolResult(toolCall.Id, JsonSerializer.Serialize(result));
    }

    private bool RequiresAuthentication(string functionName)
    {
        // Define which tools require authentication
        return functionName switch
        {
            "search_family" => true,
            "get_family_details" => true,
            "search_members" => true,
            "get_member_details" => true,
            "search_events" => true,
            "get_upcoming_events" => true,
            _ => false,
        };
    }
}