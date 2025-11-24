using backend.Application.Memories.DTOs;
using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IStoryGenerationService
{
    Task<Result<GenerateStoryResponseDto>> GenerateStoryAsync(GenerateStoryRequestDto request, CancellationToken cancellationToken);
}
