using System.Text.Json;

namespace McpServer.Models;

/// <summary>
/// Represents a part of the AI's response, which can be either text or a tool call.
/// </summary>
public class AiResponsePart
{
    /// <summary>
    /// The type of the response part ("text" or "tool_call").
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// The text content, if the type is "text".
    /// </summary>
    public string? Text { get; private set; }

    /// <summary>
    /// The tool call object, if the type is "tool_call".
    /// </summary>
    public AiToolCall? ToolCall { get; private set; }

    private AiResponsePart(string type, string? text = null, AiToolCall? toolCall = null)
    {
        Type = type;
        Text = text;
        ToolCall = toolCall;
    }

    /// <summary>
    /// Creates a text response part.
    /// </summary>
    public static AiResponsePart FromText(string text) => new("text", text: text);

    /// <summary>
    /// Creates a tool call response part.
    /// </summary>
    public static AiResponsePart FromToolCall(AiToolCall toolCall) => new("tool_call", toolCall: toolCall);

    /// <summary>
    /// Tries to parse a JSON element into an AiResponsePart.
    /// This is useful for handling responses from AI providers that might be structured differently.
    /// </summary>
    public static bool TryParse(JsonElement element, out AiResponsePart? responsePart)
    {
        responsePart = null;

        // Example parsing logic (adjust based on actual provider response format)
        if (element.TryGetProperty("text", out var textElement))
        {
            responsePart = FromText(textElement.GetString() ?? string.Empty);
            return true;
        }

        if (element.TryGetProperty("tool_calls", out var toolCallsElement) && toolCallsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var toolCallElement in toolCallsElement.EnumerateArray())
            {
                try
                {
                    var toolCall = toolCallElement.Deserialize<AiToolCall>();
                    if (toolCall != null)
                    {
                        responsePart = FromToolCall(toolCall);
                        return true; // Return on the first successfully parsed tool call
                    }
                }
                catch (JsonException)
                {
                    // Ignore if a part of the array is not a valid tool call
                }
            }
        }

        return false;
    }
}
