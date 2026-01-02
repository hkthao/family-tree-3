using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;

/// <summary>
/// Command để lưu trữ một hồ sơ giọng nói.
/// </summary>
public record ArchiveVoiceProfileCommand : IRequest<Result<VoiceProfileDto>>
{
    /// <summary>
    /// ID của hồ sơ giọng nói cần lưu trữ.
    /// </summary>
    public Guid Id { get; init; }
}
