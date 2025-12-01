using backend.Application.AI.DTOs;
using backend.Application.Common.Models;

namespace backend.Application.AI.Commands.AnalyzePhoto;

public record AnalyzePhotoCommand : IRequest<Result<PhotoAnalysisResultDto>>
{
    public AiPhotoAnalysisInputDto Input { get; init; } = default!;
}
