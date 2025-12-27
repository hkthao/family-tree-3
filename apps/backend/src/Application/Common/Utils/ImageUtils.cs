using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using Microsoft.Extensions.Logging;

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

    /// <summary>
    /// Identifies the width and height of an image from its byte array.
    /// </summary>
    /// <param name="imageBytes">The byte array of the image.</param>
    /// <param name="logger">Optional ILogger for logging warnings if dimensions cannot be identified.</param>
    /// <returns>A tuple containing the width and height (nullable integers), or null if identification fails.</returns>
    public static (int? Width, int? Height) GetImageDimensions(byte[] imageBytes, ILogger? logger = null)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            return (null, null);
        }

        try
        {
            var image = Image.Identify(imageBytes);
            if (image != null)
            {
                return (image.Width, image.Height);
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not identify image dimensions.");
        }

        return (null, null);
    }
}