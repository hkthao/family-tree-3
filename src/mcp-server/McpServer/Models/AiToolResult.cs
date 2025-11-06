namespace McpServer.Models;

/// <summary>
/// Represents the result of a single tool execution.
/// </summary>
public class AiToolResult
{
    /// <summary>
    /// The ID of the tool call that this result corresponds to.
    /// </summary>
    public string ToolCallId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the tool that was executed.
    /// </summary>
    public string ToolName { get; set; } = string.Empty;

    /// <summary>
    /// The output of the tool execution, typically a serialized object.
    /// </summary>
    public object? Output { get; set; }

    /// <summary>
    /// Indicates whether the tool execution was successful.
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// An error message if the tool execution failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
