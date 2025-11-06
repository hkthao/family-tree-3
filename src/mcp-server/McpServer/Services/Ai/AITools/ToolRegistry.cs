using McpServer.Services.Ai.AITools;

namespace McpServer.Services.Ai.AITools;

/// <summary>
/// A registry for all available AI tools.
/// </summary>
public class ToolRegistry
{
    public IReadOnlyList<ToolDefinition> Tools { get; }

    public ToolRegistry()
    {
        Tools = new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Name = "search_families",
                Description = "Searches for families by a query string.",
                Parameters = new 
                {
                    type = "object",
                    properties = new 
                    {
                        query = new { type = "string", description = "The search query for families." }
                    },
                    required = new[] { "query" }
                }
            },
            new ToolDefinition
            {
                Name = "get_family_details",
                Description = "Gets the details of a specific family by its ID.",
                Parameters = new 
                {
                    type = "object",
                    properties = new 
                    {
                        familyId = new { type = "string", format = "uuid", description = "The ID of the family." }
                    },
                    required = new[] { "familyId" }
                }
            },
            new ToolDefinition
            {
                Name = "search_members",
                Description = "Searches for members by a query string, optionally within a specific family.",
                Parameters = new 
                {
                    type = "object",
                    properties = new 
                    {
                        query = new { type = "string", description = "The search query for members." },
                        familyId = new { type = "string", format = "uuid", description = "(Optional) The ID of the family to search within." }
                    },
                    required = new[] { "query" }
                }
            },
            new ToolDefinition
            {
                Name = "get_member_details",
                Description = "Gets the details of a specific member by their ID.",
                Parameters = new 
                {
                    type = "object",
                    properties = new 
                    {
                        memberId = new { type = "string", format = "uuid", description = "The ID of the member." }
                    },
                    required = new[] { "memberId" }
                }
            },
            new ToolDefinition
            {
                Name = "search_events",
                Description = "Searches for events by a query string, optionally filtered by family, start date, and end date.",
                Parameters = new 
                {
                    type = "object",
                    properties = new 
                    {
                        query = new { type = "string", description = "The search query for events." },
                        familyId = new { type = "string", format = "uuid", description = "(Optional) The ID of the family to filter by." },
                        startDate = new { type = "string", format = "date-time", description = "(Optional) The start date for the search range." },
                        endDate = new { type = "string", format = "date-time", description = "(Optional) The end date for the search range." }
                    },
                    required = new[] { "query" }
                }
            },
            new ToolDefinition
            {
                Name = "get_upcoming_events",
                Description = "Gets a list of upcoming events, optionally filtered by family.",
                Parameters = new 
                {
                    type = "object",
                    properties = new 
                    {
                        familyId = new { type = "string", format = "uuid", description = "(Optional) The ID of the family to get upcoming events for." }
                    }
                }
            }
        }.AsReadOnly();
    }
}
