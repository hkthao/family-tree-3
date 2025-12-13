using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;
using backend.Domain.Enums;

namespace backend.Application.MemberStories.Commands.CreateMemberStory;

public record CreateMemberStoryCommand : IRequest<Result<Guid>>
{
    public Guid MemberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Story { get; init; } = string.Empty;
    public int? Year { get; init; }
    public string? TimeRangeDescription { get; init; }
    public bool IsYearEstimated { get; init; }
    public LifeStage LifeStage { get; init; }
    public string? Location { get; init; }
    public Guid? StorytellerId { get; init; }
    public CertaintyLevel CertaintyLevel { get; init; }
    public List<DetectedFaceDto> DetectedFaces { get; init; } = new List<DetectedFaceDto>();
    // Property to hold image URLs that will be converted to MemberStoryImage entities
    public string? TemporaryOriginalImageUrl { get; init; }
    public string? TemporaryResizedImageUrl { get; init; }
}
