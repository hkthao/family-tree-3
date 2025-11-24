using backend.Application.Memories.DTOs;
using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IPhotoAnalysisService
{
    Task<Result<PhotoAnalysisResultDto>> AnalyzePhotoAsync(AnalyzePhotoRequestDto request, CancellationToken cancellationToken);
}
