using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;

namespace backend.Application.Memories.Commands.AnalyzePhoto;

public record AnalyzePhotoCommand : IRequest<Result<PhotoAnalysisResultDto>>
{
    public AiPhotoAnalysisInputDto Input { get; init; } = default!;
}
