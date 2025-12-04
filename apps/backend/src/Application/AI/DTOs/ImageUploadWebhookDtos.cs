using System.Text.Json.Serialization;

namespace backend.Application.AI.DTOs;

/// <summary>
/// DTO cho payload webhook tải ảnh lên.
/// </summary>
public class ImageUploadWebhookDto
{
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string Folder { get; set; } = string.Empty; // e.g., "test"
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>
/// DTO cho phản hồi từ webhook tải ảnh lên.
/// </summary>
public class ImageUploadResponseDto
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("display_url")]
    public string DisplayUrl { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("extension")]
    public string Extension { get; set; } = string.Empty;

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = string.Empty;
}
