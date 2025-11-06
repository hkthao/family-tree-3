using System.Text.Json.Serialization;

namespace McpServer.Models;

/// <summary>
/// Represents a message in the conversation history.
/// </summary>
public class AiMessage
{
    /// <summary>
    /// The role of the message sender (e.g., "user", "assistant", "tool").
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional: A list of tool calls requested by the assistant.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<AiToolCall>? ToolCalls { get; set; }

    /// <summary>
    /// Optional: The ID of the tool call this message is a response to.
    /// </summary>
    [JsonPropertyName("tool_call_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolCallId { get; set; }

    public AiMessage() { }

    public AiMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }
}

/// <summary>
/// Defines the roles for AI messages.
/// </summary>
public enum AiMessageRole
{
    System,
    User,
    Assistant,
    Tool
}