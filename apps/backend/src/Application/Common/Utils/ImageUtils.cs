using System.Text.RegularExpressions;

namespace backend.Application.Common.Utils;

public static class ImageUtils
{
    /// <summary>
    /// Converts a base64 string to a byte array, handling data URI prefixes.
    /// </summary>
    /// <param name="base64String">The base64 string, potentially with a data URI prefix (e.g., "data:image/jpeg;base64,").</param>
    /// <returns>A byte array representing the decoded base64 string.</returns>
    public static byte[] ConvertBase64ToBytes(string base64String)
    {
        // Remove "data:image/jpeg;base64," or "data:image/png;base64," prefix if present
        var base64Data = Regex.Match(base64String, @"data:image/(?<type>.+?);base64,(?<data>.+)").Groups["data"].Value;
        if (string.IsNullOrEmpty(base64Data))
        {
            base64Data = base64String; // Fallback if no prefix or not an image data URI
        }
        return Convert.FromBase64String(base64Data);
    }
}
