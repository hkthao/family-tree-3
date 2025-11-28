using backend.Application.Common.Models;
using backend.Application.MemberStories.Commands.GenerateStory; // Added
using backend.Application.MemberStories.DTOs; // Updated

namespace backend.Application.Common.Interfaces;

public interface IStoryGenerationService
{
    Task<Result<GenerateStoryResponseDto>> GenerateStoryAsync(GenerateStoryRequestDto request, CancellationToken cancellationToken);
}
