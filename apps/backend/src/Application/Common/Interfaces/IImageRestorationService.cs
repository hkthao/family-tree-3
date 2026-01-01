using backend.Application.Common.Models; // For Result
using backend.Application.Common.Models.ImageRestoration; // Added

namespace backend.Application.Common.Interfaces;

public interface IImageRestorationService
{
    Task<Result<StartImageRestorationResponseDto>> StartRestorationAsync(string imageUrl, CancellationToken cancellationToken = default);
    Task<Result<ImageRestorationJobStatusDto>> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken = default);
}
