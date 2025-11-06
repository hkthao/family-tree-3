using System.Text.Json.Serialization;

namespace McpServer.Models;

/// <summary>
/// Represents a tool call suggested by the Language Model.
/// </summary>
public class AiToolCall
{
    /// <summary>
    /// A unique identifier for the tool call.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The type of the tool, always "function".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    /// <summary>
    /// The function to be called.
    /// </summary>
    [JsonPropertyName("function")]
    public AiToolFunction Function { get; set; } = new();
}

/// <summary>
/// Represents the function details of a tool call.
/// </summary>
public class AiToolFunction
{
    /// <summary>
    /// The name of the function to call.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The arguments to the function, as a JSON string.
    /// </summary>
    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}
