using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.Files.DeleteFile;

public record DeleteFileCommand : IRequest<Result>
{
    public Guid FileId { get; init; }
}
