using backend.Application.Common.Models; // For Result
using backend.Application.Common.Models.ImageRestoration; // Added

namespace backend.Application.Common.Interfaces;

public interface IImageRestorationService
{
    Task<Result<StartImageRestorationResponseDto>> StartRestorationAsync(string imageUrl, bool useCodeformer, CancellationToken cancellationToken = default);
    Task<Result<ImageRestorationJobStatusDto>> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<Result<PreprocessImageResponseDto>> PreprocessImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken cancellationToken = default);
}
