using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;

namespace backend.Application.Common.Interfaces;

public interface IStoryGenerationService
{
    Task<Result<GenerateStoryResponseDto>> GenerateStoryAsync(GenerateStoryRequestDto request, CancellationToken cancellationToken);
}
