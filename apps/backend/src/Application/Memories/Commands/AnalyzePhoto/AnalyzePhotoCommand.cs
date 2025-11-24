using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;
using Microsoft.AspNetCore.Http; // For IFormFile

namespace backend.Application.Memories.Commands.AnalyzePhoto;

public record AnalyzePhotoCommand : IRequest<Result<PhotoAnalysisResultDto>>
{
    public IFormFile File { get; init; } = default!;
    public Guid? MemberId { get; init; }
}
