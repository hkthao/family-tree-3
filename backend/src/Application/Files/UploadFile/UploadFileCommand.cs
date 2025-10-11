using backend.Application.Common.Models;

namespace backend.Application.Files.UploadFile
{
    /// <summary>
    /// Command to upload a file.
    /// </summary>
    public record UploadFileCommand : IRequest<Result<string>>
    {
        /// <summary>
        /// The stream of the file to upload.
        /// </summary>
        public Stream FileStream { get; init; } = null!;

        /// <summary>
        /// The original name of the file.
        /// </summary>
        public string FileName { get; init; } = null!;

        /// <summary>
        /// The content type of the file.
        /// </summary>
        public string ContentType { get; init; } = null!;

        /// <summary>
        /// The length of the file in bytes.
        /// </summary>
        public long Length { get; init; }
    }
}
