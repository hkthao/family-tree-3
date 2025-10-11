using backend.Application.Common.Models;

namespace backend.Application.Files.DeleteFile
{
    public record DeleteFileCommand : IRequest<Result>
    {
        public Guid FileId { get; init; }
    }
}
