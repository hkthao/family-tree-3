namespace McpServer.Services.Ai.AITools;

/// <summary>
/// Defines the structure of a tool that can be called by the AI.
/// </summary>
public class ToolDefinition
{
    /// <summary>
    /// The name of the tool.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A description of what the tool does.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The parameters the tool accepts, defined as a JSON schema.
    /// </summary>
    public object Parameters { get; set; } = new { };
}
