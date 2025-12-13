using System.IO;
using backend.Domain.Enums;

namespace backend.Application.Common.Extensions;

public static class MediaTypeExtensions
{
    public static MediaType InferMediaType(this string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return MediaType.Other;
        }

        string extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;

        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => MediaType.Image,
            ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" => MediaType.Video,
            ".mp3" or ".wav" or ".aac" or ".flac" => MediaType.Audio,
            ".pdf" => MediaType.Document,
            _ => MediaType.Other
        };
    }
}
