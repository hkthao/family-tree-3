using backend.Application.Common.Models;

namespace backend.Application.Files.Queries.GetUploadedFile;

public class GetUploadedFileQuery : IRequest<Result<FileContentDto>>
{
    public string FileName { get; set; } = null!;
}

public class FileContentDto
{
    public byte[] Content { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}
