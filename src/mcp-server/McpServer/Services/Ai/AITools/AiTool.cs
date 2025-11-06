using System.Text.Json.Serialization;

namespace McpServer.Services.Ai.Tools;

// Lớp cơ sở cho các phản hồi từ AI, có thể là text hoặc tool call
public abstract record AiResponsePart;

// Phần phản hồi chứa text thông thường
public record AiTextResponsePart(string Text) : AiResponsePart;

// Phần phản hồi yêu cầu gọi một hoặc nhiều tool
public record AiToolCallResponsePart(List<AiToolCall> ToolCalls) : AiResponsePart;


/// <summary>
/// Đại diện cho một yêu cầu gọi hàm (tool call) từ LLM.
/// </summary>
/// <param name="Id">ID của lời gọi, dùng để đối chiếu với kết quả.</param>
/// <param name="FunctionName">Tên của hàm cần gọi.</param>
/// <param name="FunctionArgs">Các đối số của hàm dưới dạng JSON string.</param>
public record AiToolCall(string Id, string FunctionName, string FunctionArgs);

/// <summary>
/// Đại diện cho kết quả của một tool sau khi được thực thi.
/// </summary>
/// <param name="ToolCallId">ID của lời gọi tool tương ứng.</param>
/// <param name="Content">Kết quả của tool dưới dạng string (thường là JSON).</param>
public record AiToolResult(string ToolCallId, string Content);

/// <summary>
/// Định nghĩa cấu trúc của một tool để gửi cho LLM.
/// </summary>
public class AiToolDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public AiToolParameters Parameters { get; set; } = new();
}

public class AiToolParameters
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, AiToolParameterProperty> Properties { get; set; } = new();

    [JsonPropertyName("required")]
    public List<string> Required { get; set; } = new();
}

public class AiToolParameterProperty
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // e.g., "string", "number"

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
