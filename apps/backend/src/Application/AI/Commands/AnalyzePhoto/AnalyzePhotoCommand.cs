using backend.Application.Common.Models;
using backend.Application.AI.DTOs;

namespace backend.Application.AI.Commands.AnalyzePhoto;

public record AnalyzePhotoCommand : IRequest<Result<PhotoAnalysisResultDto>>
{
    public AiPhotoAnalysisInputDto Input { get; init; } = default!;
}
