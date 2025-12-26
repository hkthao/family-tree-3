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

    /// <summary>
    /// Infers the media type from a given URL.
    /// </summary>
    /// <param name="url">The URL from which to infer the media type.</param>
    /// <returns>The inferred MediaType.</returns>
    public static MediaType InferMediaTypeFromUrl(this string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return MediaType.Other;
        }

        Uri? uri;
        if (Uri.TryCreate(url, UriKind.Absolute, out uri))
        {
            string path = uri.AbsolutePath;
            string fileNameWithExtension = Path.GetFileName(path);
            string extension = Path.GetExtension(fileNameWithExtension).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension))
            {
                return MediaType.Other;
            }

            return extension.InferMediaType();
        }

        return MediaType.Other;
    }

}
