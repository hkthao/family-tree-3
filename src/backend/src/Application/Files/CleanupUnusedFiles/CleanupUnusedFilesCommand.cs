using backend.Application.Common.Models;

namespace backend.Application.Files.CleanupUnusedFiles;

public record CleanupUnusedFilesCommand : IRequest<Result<int>>
{
    public TimeSpan OlderThan { get; init; } = TimeSpan.FromDays(30); // Default to files older than 30 days
}
